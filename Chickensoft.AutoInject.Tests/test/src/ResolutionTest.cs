namespace Chickensoft.AutoInject.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chickensoft.AutoInject.Tests.Subjects;
using Chickensoft.GoDotTest;
using Chickensoft.GodotTestDriver;
using Godot;
using Shouldly;

public class ResolutionTest(Node testScene) : TestClass(testScene) {
  [Test]
  public void Provides() {
    var value = "Hello, world!";
    var provider = new StringProvider() { Value = value };

    ((IProvide<string>)provider).Value().ShouldBe(value);

    provider._Notification((int)Node.NotificationReady);

    provider.OnProvidedCalled.ShouldBeTrue();
  }

  [Test]
  public void ProviderResetsOnTreeExit() {
    var value = "Hello, world!";
    var obj = new StringProvider() { Value = value };
    var provider = obj as IBaseProvider;

    ((IProvide<string>)provider).Value().ShouldBe(value);

    obj._Notification((int)Node.NotificationReady);
    provider.ProviderState.IsInitialized.ShouldBeTrue();

    obj._Notification((int)Node.NotificationExitTree);
    provider.ProviderState.IsInitialized.ShouldBeFalse();
  }

  [Test]
  public void ResolvesDependencyWhenProviderIsAlreadyInitialized() {
    var value = "Hello, world!";
    var obj = new StringProvider() { Value = value };
    var provider = obj as IBaseProvider;
    var dependent = new StringDependent();

    obj.AddChild(dependent);

    ((IProvide<string>)provider).Value().ShouldBe(value);

    obj._Notification((int)Node.NotificationReady);
    provider.ProviderState.IsInitialized.ShouldBeTrue();
    obj.OnProvidedCalled.ShouldBeTrue();

    dependent._Notification((int)Node.NotificationReady);

    dependent.OnResolvedCalled.ShouldBeTrue();
    dependent.ResolvedValue.ShouldBe(value);

    obj.RemoveChild(dependent);
    dependent.QueueFree();
    obj.QueueFree();
  }

  [Test]
  public async Task ResolvesDependencyAfterProviderIsResolved() {
    var value = "Hello, world!";
    var obj = new StringProvider() { Value = value };
    var provider = obj as IBaseProvider;
    var dependent = new StringDependent();
    var fixture = new Fixture(TestScene.GetTree());
    obj.AddChild(dependent);

    await fixture.AddToRoot(obj);

    ((IProvide<string>)provider).Value().ShouldBe(value);

    provider.ProviderState.IsInitialized.ShouldBeTrue();
    obj.OnProvidedCalled.ShouldBeTrue();

    dependent.OnResolvedCalled.ShouldBeTrue();
    dependent.ResolvedValue.ShouldBe(value);
    ((IDependent)dependent).DependentState.Pending.ShouldBeEmpty();

    await fixture.Cleanup();

    obj.RemoveChild(dependent);
    dependent.QueueFree();
    obj.QueueFree();
  }

  [Test]
  public async Task FindsDependenciesAcrossAncestors() {
    var value = "Hello, world!";

    var objA = new StringProvider() { Value = value };
    var providerA = objA as IBaseProvider;
    var objB = new IntProvider() { Value = 10 };
    var providerB = objB as IBaseProvider;
    var depObj = new StringDependent();
    var dependent = depObj as IDependent;
    var fixture = new Fixture(TestScene.GetTree());

    objA.AddChild(objB);
    objA.AddChild(depObj);

    await fixture.AddToRoot(objA);

    providerA.ProviderState.IsInitialized.ShouldBeTrue();
    objA.OnProvidedCalled.ShouldBeTrue();

    providerB.ProviderState.IsInitialized.ShouldBeTrue();
    objB.OnProvidedCalled.ShouldBeTrue();

    depObj.OnResolvedCalled.ShouldBeTrue();
    depObj.ResolvedValue.ShouldBe(value);
    dependent.DependentState.Pending.ShouldBeEmpty();

    objA.RemoveChild(objB);
    objB.RemoveChild(depObj);
    depObj.QueueFree();
    objB.QueueFree();
    objA.QueueFree();
  }

  [Test]
  public void ThrowsWhenNoProviderFound() {
    var dependent = new StringDependent();

    Should.Throw<ProviderNotFoundException>(
      () => dependent._Notification((int)Node.NotificationReady)
    );
  }

  [Test]
  public void UsesReferenceFallbackValueWhenNoProviderFound() {
    var fallback = new Resource();
    var dependent = new ReferenceDependentFallback {
      FallbackValue = fallback
    };

    dependent._Notification((int)Node.NotificationReady);

    dependent.ResolvedValue.ShouldBe(fallback);
    dependent.MyDependency.ShouldBe(fallback);
  }

  [Test]
  public void DependsOnValueType() {
    var value = 10;
    var depObj = new IntDependent() { FallbackValue = () => value };
    var dependent = depObj as IDependent;

    depObj._Notification((int)Node.NotificationReady);


    depObj.OnResolvedCalled.ShouldBeTrue();
    depObj.ResolvedValue.ShouldBe(value);

    depObj._Notification((int)Node.NotificationExitTree);

    dependent.DependentState.Pending.ShouldBeEmpty();

    depObj.QueueFree();
  }

  [Test]
  public void ThrowsIfFallbackProducesNullAfterPreviousValueIsGarbageCollected(
  ) {
    var currentFallback = 0;
    var replacementValue = new object();
    var fallbacks = new List<object?>() { new(), null, replacementValue };

    var value = Utils.CreateWeakReference();

    // Fallback will be called 3 times in this test. First will be non-null,
    // second will be null, third will be non-null and different from the first.
    var depObj = new WeakReferenceDependent() {
      Fallback = () => fallbacks[currentFallback++]!
    };

    var dependent = depObj as IDependent;

    depObj._Notification((int)Node.NotificationReady);

    // Let's access the fallback value to ensure the default provider is setup.
    depObj.MyDependency.ShouldNotBeNull();

    // Simulate a garbage collected object. We support weak references to
    // dependencies to avoid causing build issues when reloading the scene.
    Utils.ClearWeakReference(value);

    // To test this highly specific scenario, we have to clear ALL
    // weak references to the object, including the one in the default provider
    // that's generated behind-the-scenes for us.

    // Let's dig out the weak ref used in the default provider from the
    // dependent's internal state...
    var underlyingDefaultProvider =
      (DependencyResolver.DefaultProvider<object>)
        depObj.MixinState.Get<DependentState>().Dependencies[typeof(object)];

    var actualWeakRef = (WeakReference)underlyingDefaultProvider._value;

    Utils.ClearWeakReference(actualWeakRef);

    var e = Should.Throw<InvalidOperationException>(
      () => depObj.MyDependency
    );

    e.Message.ShouldContain("cannot create a null value");

    // Now that the fallback returns a valid value, the dependency should
    // be resolved once again.
    depObj.MyDependency.ShouldBeSameAs(replacementValue);
  }

  [Test]
  public void ThrowsOnDependencyTableThatWasTamperedWith() {
    var fallback = "Hello, world!";
    var depObj = new StringDependentFallback {
      FallbackValue = fallback
    };
    var dependent = depObj as IDependent;

    depObj._Notification((int)Node.NotificationReady);

    dependent.DependentState.Dependencies[typeof(string)] = new BadProvider();

    Should.Throw<ProviderNotFoundException>(
      () => depObj.MyDependency.ShouldBe(fallback)
    );
  }

  [Test]
  public void DependentCancelsPendingIfRemovedFromTree() {
    var provider = new StringProvider();
    var depObj = new StringDependent();
    var dependent = depObj as IDependent;

    provider.AddChild(depObj);

    depObj._Notification((int)Node.NotificationReady);

    dependent.DependentState.Pending.ShouldNotBeEmpty();

    depObj._Notification((int)Node.NotificationExitTree);

    dependent.DependentState.Pending.ShouldBeEmpty();

    provider.RemoveChild(depObj);
    depObj.QueueFree();
    provider.QueueFree();
  }

  [Test]
  public void AccessingDependencyBeforeProvidedEvenIfCreatedThrows() {
    // Accessing a dependency that might already be available (but the provider
    // hasn't called Provide() yet) should throw an exception.

    var provider = new StringProvider();
    var dependent = new StringDependent();

    provider.AddChild(dependent);

    dependent._Notification((int)Node.NotificationReady);
    Should.Throw<ProviderNotInitializedException>(() => dependent.MyDependency);
  }

  [Test]
  public void DependentWithNoDependenciesHasOnResolvedCalled() {
    var provider = new StringProvider();
    var dependent = new NoDependenciesDependent();

    provider.AddChild(dependent);

    dependent._Notification((int)Node.NotificationReady);

    dependent.OnResolvedCalled.ShouldBeTrue();
  }

  [Test]
  public void FakesDependency() {
    var dependent = new FakedDependent();

    var fakeValue = "I'm fake!";
    dependent.FakeDependency(fakeValue);

    TestScene.AddChild(dependent);

    dependent._Notification((int)Node.NotificationReady);

    dependent.OnResolvedCalled.ShouldBeTrue();
    dependent.MyDependency.ShouldBe(fakeValue);

    TestScene.RemoveChild(dependent);
  }

  public class BadProvider : IBaseProvider {
    public ProviderState ProviderState { get; }

    public BadProvider() {
      ProviderState = new ProviderState {
        IsInitialized = true
      };
    }
  }

  public static class Utils {
    public static WeakReference CreateWeakReference() => new(new object());

    public static void ClearWeakReference(WeakReference weakReference) {
      weakReference.Target = null;

      while (weakReference.Target is not null) {
        GC.Collect();
        GC.WaitForPendingFinalizers();
      }
    }
  }
}
