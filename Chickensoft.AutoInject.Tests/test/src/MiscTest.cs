namespace Chickensoft.AutoInject.Tests;

using System;
using Chickensoft.AutoInject.Tests.Subjects;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public partial class TestDependent : Dependent { }

public class MiscTest : TestClass {
  public MiscTest(Node testScene) : base(testScene) { }

  [Test]
  public void DependentStubs() {
    Dependent.ScriptPropertiesAndFields.ShouldNotBeNull();
    Dependent.ReceiveScriptPropertyOrFieldType<int>(
      default!, default!
    ).ShouldBe(default!);

    var dependent = new TestDependent();

    // dependent.AllDependencies

    Should.Throw<NotImplementedException>(
      () => dependent.PropertiesAndFields
    );
    Should.Throw<NotImplementedException>(
      () => dependent.GetScriptPropertyOrFieldType<int>("a", default!)
    );
    Should.Throw<NotImplementedException>(
      () => dependent.GetScriptPropertyOrField("a")
    );
    Should.Throw<NotImplementedException>(
      () => dependent.SetScriptPropertyOrField("a", default!)
    );

    dependent.QueueFree();
  }

  [Test]
  public void DependencyPendingCancels() {
    var provider = new StringProvider();
    var initialized = false;
    void onInitialized(IProvider provider) => initialized = true;

    provider.ProviderState.OnInitialized += onInitialized;

    var pending = new PendingProvider(provider, onInitialized);

    pending.Unsubscribe();

    provider.ProviderState.Announce(provider);

    initialized.ShouldBeFalse();

    provider.QueueFree();
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
    var defaultProvider = new DependencyResolver.DefaultProvider("hi");
    defaultProvider.ProviderState.ShouldNotBeNull();
  }
}
