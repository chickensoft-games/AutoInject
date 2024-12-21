namespace Chickensoft.AutoInject.Tests;

using System;
using Chickensoft.AutoInject.Tests.Subjects;
using Chickensoft.GoDotTest;
using Chickensoft.Introspection;
using Godot;
using Shouldly;

[Meta(typeof(IAutoOn), typeof(IDependent))]
public partial class TestDependent { }

public class MiscTest(Node testScene) : TestClass(testScene) {
  [Test]
  public void DependencyPendingCancels() {
    var obj = new StringProvider();
    var provider = obj as IBaseProvider;
    var initialized = false;
    void onInitialized(IBaseProvider provider) => initialized = true;

    provider.ProviderState.OnInitialized += onInitialized;

    var pending = new PendingProvider(provider, onInitialized);

    pending.Unsubscribe();

    provider.ProviderState.Announce(provider);

    initialized.ShouldBeFalse();

    obj.QueueFree();
  }

  [Test]
  public void ProviderNotFoundException() {
    var exception = new ProviderNotFoundException(typeof(ObsoleteAttribute));

    exception.Message.ShouldContain(nameof(ObsoleteAttribute));
  }

  [Test]
  public void IDependentOnResolvedDoesNothing() {
    var dependent = new TestDependent();

    Should.NotThrow(() => ((IDependent)dependent).OnResolved());
  }

  [Test]
  public void DefaultProviderState() {
    var defaultProvider = new DependencyResolver.DefaultProvider<string>("hi");
    defaultProvider.ProviderState.ShouldNotBeNull();
  }
}
