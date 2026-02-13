namespace Chickensoft.AutoInject.Tests;

using Chickensoft.GoDotTest;
using Godot;
using Shouldly;



public partial class NotificationExtensionsTest(
  Node testScene
) : TestClass(testScene)
{
  [Test]
  public void DoesNothingIfNotIntrospective()
  {
    var node = new Node();

    Should.NotThrow(() => node.Notify(1));

    node.QueueFree();
  }
}
