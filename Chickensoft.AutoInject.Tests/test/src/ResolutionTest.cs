namespace Chickensoft.AutoInject.Tests;

using Chickensoft.AutoInject.Tests.Subjects;
using Chickensoft.GoDotTest;
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
  public void ResolvesDependencyAfterProviderIsResolved() {
    var value = "Hello, world!";
    var obj = new StringProvider() { Value = value };
    var provider = obj as IBaseProvider;
    var dependent = new StringDependent();

    obj.AddChild(dependent);

    ((IProvide<string>)provider).Value().ShouldBe(value);

    dependent._Notification((int)Node.NotificationReady);

    obj._Notification((int)Node.NotificationReady);
    provider.ProviderState.IsInitialized.ShouldBeTrue();
    obj.OnProvidedCalled.ShouldBeTrue();

    dependent.OnResolvedCalled.ShouldBeTrue();
    dependent.ResolvedValue.ShouldBe(value);
    ((IDependent)dependent).DependentState.Pending.ShouldBeEmpty();

    obj.RemoveChild(dependent);
    dependent.QueueFree();
    obj.QueueFree();
  }

  [Test]
  public void FindsDependenciesAcrossAncestors() {
    var value = "Hello, world!";

    var objA = new StringProvider() { Value = value };
    var providerA = objA as IBaseProvider;
    var objB = new IntProvider() { Value = 10 };
    var providerB = objB as IBaseProvider;
    var depObj = new StringDependent();
    var dependent = depObj as IDependent;

    objA.AddChild(objB);
    objA.AddChild(depObj);

    depObj._Notification((int)Node.NotificationReady);

    objA._Notification((int)Node.NotificationReady);
    providerA.ProviderState.IsInitialized.ShouldBeTrue();
    objA.OnProvidedCalled.ShouldBeTrue();

    objB._Notification((int)Node.NotificationReady);
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
  public void UsesFallbackValueWhenNoProviderFound() {
    var fallback = "Hello, world!";
    var dependent = new StringDependentFallback {
      FallbackValue = fallback
    };

    dependent._Notification((int)Node.NotificationReady);

    dependent.ResolvedValue.ShouldBe(fallback);
    dependent.MyDependency.ShouldBe(fallback);
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
}
