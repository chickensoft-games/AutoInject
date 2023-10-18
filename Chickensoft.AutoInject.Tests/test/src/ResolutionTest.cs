namespace Chickensoft.AutoInject.Tests;

using Chickensoft.AutoInject.Tests.Subjects;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class ResolutionTest : TestClass {
  public ResolutionTest(Node testScene) : base(testScene) { }

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
    var provider = new StringProvider() { Value = value };

    ((IProvide<string>)provider).Value().ShouldBe(value);

    provider._Notification((int)Node.NotificationReady);

    provider.ProviderState.IsInitialized.ShouldBeTrue();

    provider._Notification((int)Node.NotificationExitTree);

    provider.ProviderState.IsInitialized.ShouldBeFalse();
  }

  [Test]
  public void ResolvesDependencyWhenProviderIsAlreadyInitialized() {
    var value = "Hello, world!";
    var provider = new StringProvider() { Value = value };
    var dependent = new StringDependent();

    provider.AddChild(dependent);

    ((IProvide<string>)provider).Value().ShouldBe(value);

    provider._Notification((int)Node.NotificationReady);
    provider.ProviderState.IsInitialized.ShouldBeTrue();
    provider.OnProvidedCalled.ShouldBeTrue();

    dependent._Notification((int)Node.NotificationReady);

    dependent.OnResolvedCalled.ShouldBeTrue();
    dependent.ResolvedValue.ShouldBe(value);

    provider.RemoveChild(dependent);
    dependent.QueueFree();
    provider.QueueFree();
  }

  [Test]
  public void ResolvesDependencyAfterProviderIsResolved() {
    var value = "Hello, world!";
    var provider = new StringProvider() { Value = value };
    var dependent = new StringDependent();

    provider.AddChild(dependent);

    ((IProvide<string>)provider).Value().ShouldBe(value);

    dependent._Notification((int)Node.NotificationReady);

    provider._Notification((int)Node.NotificationReady);
    provider.ProviderState.IsInitialized.ShouldBeTrue();
    provider.OnProvidedCalled.ShouldBeTrue();

    dependent.OnResolvedCalled.ShouldBeTrue();
    dependent.ResolvedValue.ShouldBe(value);
    dependent.DependentState.Pending.ShouldBeEmpty();

    provider.RemoveChild(dependent);
    dependent.QueueFree();
    provider.QueueFree();
  }

  [Test]
  public void FindsDependenciesAcrossAncestors() {
    var value = "Hello, world!";
    var providerA = new StringProvider() { Value = value };
    var providerB = new IntProvider() { Value = 10 };
    var dependent = new StringDependent();

    var onResolvedCalled = false;
    void onResolved() =>
      onResolvedCalled = true;

    dependent.OnDependenciesResolved += onResolved;

    providerA.AddChild(providerB);
    providerB.AddChild(dependent);

    dependent._Notification((int)Node.NotificationReady);

    providerA._Notification((int)Node.NotificationReady);
    providerA.ProviderState.IsInitialized.ShouldBeTrue();
    providerA.OnProvidedCalled.ShouldBeTrue();

    onResolvedCalled.ShouldBeTrue();

    providerB._Notification((int)Node.NotificationReady);
    providerB.ProviderState.IsInitialized.ShouldBeTrue();
    providerB.OnProvidedCalled.ShouldBeTrue();

    dependent.OnResolvedCalled.ShouldBeTrue();
    dependent.ResolvedValue.ShouldBe(value);
    dependent.DependentState.Pending.ShouldBeEmpty();

    providerA.RemoveChild(providerB);
    providerB.RemoveChild(dependent);
    dependent.QueueFree();
    providerB.QueueFree();
    providerA.QueueFree();
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
    var dependent = new StringDependentFallback {
      FallbackValue = fallback
    };

    dependent._Notification((int)Node.NotificationReady);

    dependent.DependentState.Dependencies[typeof(string)] = new BadProvider();

    Should.Throw<ProviderNotFoundException>(
      () => dependent.MyDependency.ShouldBe(fallback)
    );
  }

  [Test]
  public void DependentCancelsPendingIfRemovedFromTree() {
    var provider = new StringProvider();
    var dependent = new StringDependent();

    provider.AddChild(dependent);

    dependent._Notification((int)Node.NotificationReady);

    dependent.DependentState.Pending.ShouldNotBeEmpty();

    dependent._Notification((int)Node.NotificationExitTree);

    dependent.DependentState.Pending.ShouldBeEmpty();

    provider.RemoveChild(dependent);
    dependent.QueueFree();
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

  public class BadProvider : IProvider {
    public ProviderState ProviderState { get; }

    public BadProvider() {
      ProviderState = new ProviderState {
        IsInitialized = true
      };
    }
  }
}
