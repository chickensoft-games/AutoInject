namespace Chickensoft.AutoInject.Tests;
using Chickensoft.GoDotTest;
using Chickensoft.AutoInject.Tests.Fixtures;
using Godot;
using Shouldly;
using Chickensoft.Introspection;

public partial class AutoInitTest(Node testScene) : TestClass(testScene) {
  [Meta(typeof(IAutoInit))]
  public partial class NotAGodotNode { }

  [Test]
  public void SetsUpNode() {
    var node = new AutoInitTestNode();

    node._Notification((int)Node.NotificationReady);

    node.SetupCalled.ShouldBeTrue();
  }

  [Test]
  public void DefaultImplementationDoesNothing() {
    var node = new AutoInitTestNodeNoImplementation();

    node._Notification((int)Node.NotificationReady);
  }

  [Test]
  public void IsTestingCreatesStateIfSetFirst() {
    var node = new AutoInitTestNode();
    (node as IAutoInit).IsTesting = true;
  }

  [Test]
  public void HandlerDoesNotWorkIfNotGodotNode() => Should.NotThrow(() => {
    var node = new NotAGodotNode();
    (node as IAutoInit).Handler();
  });
}
