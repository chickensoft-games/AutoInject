namespace Chickensoft.AutoInject.Tests;
using Chickensoft.GoDotTest;
using Chickensoft.Introspection;
using Godot;
using Shouldly;

public partial class AutoNodeTest(Node testScene) : TestClass(testScene) {
  [Meta(typeof(IAutoNode))]
  public partial class NotAGodotNode : GodotObject { }


  [Test]
  public void MixinHandlerActuallyDoesNothing() {
    IMixin<IAutoNode> node = new NotAGodotNode();

    Should.NotThrow(node.Handler);
  }

  [Test]
  public void CallsOtherMixins() => Should.NotThrow(() => {

    var node = new NotAGodotNode();

    node.__SetupNotificationStateIfNeeded();

    IIntrospectiveRef introspective = node;

    // Some mixins need this data.
    node.MixinState.Get<NotificationState>().Notification = -1;

    introspective.InvokeMixins();
  });
}
