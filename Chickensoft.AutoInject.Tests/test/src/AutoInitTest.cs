namespace Chickensoft.AutoInject.Tests;

using Chickensoft.AutoInject.Tests.Fixtures;
using Chickensoft.GoDotTest;
using Chickensoft.Introspection;
using Godot;
using Shouldly;

public partial class AutoInitTest(Node testScene) : TestClass(testScene)
{
  [Meta(typeof(IAutoInit))]
  public partial class NotAGodotNode { }

  [Test]
  public void SetsUpNode()
  {
    var node = new AutoInitTestNode();

    node._Notification((int)Node.NotificationReady);

    node.Called.ShouldBe(1);

    node.QueueFree();
  }

  [Test]
  public void DefaultImplementationDoesNothing()
  {
    var node = new AutoInitTestNodeNoImplementation();

    node._Notification((int)Node.NotificationReady);

    node.QueueFree();
  }

  [Test]
  public void IsTestingCreatesStateIfSetFirst()
  {
    var node = new AutoInitTestNode();
    (node as IAutoInit).IsTesting = true;
    // Should do nothing on a non-ready notification
    node._Notification((int)Node.NotificationEnterTree);

    node.QueueFree();
  }

  [Test]
  public void HandlerDoesNotWorkIfNotGodotNode() => Should.NotThrow(() =>
  {
    var node = new NotAGodotNode();
    (node as IAutoInit).Handler();
  });

  [Test]
  public void AutoNodeMixinOnlyCallsInitializeOnce()
  {
    var node = new AutoInitTestAutoNode();

    node._Notification((int)Node.NotificationReady);

    node.Called.ShouldBe(1);

    node.QueueFree();
  }
}
