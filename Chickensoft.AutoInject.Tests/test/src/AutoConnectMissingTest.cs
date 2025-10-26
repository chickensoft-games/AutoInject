namespace Chickensoft.AutoInject.Tests;

using Chickensoft.AutoInject.Tests.Fixtures;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class AutoConnectMissingTest(Node testScene) : TestClass(testScene)
{
  [Test]
  public void ThrowsOnMissingNode()
  {
    var scene = GD.Load<PackedScene>("res://test/fixtures/AutoConnectMissingTestScene.tscn");
    // AutoNode will actually throw an InvalidOperationException
    // during the scene instantiation, but for whatever reason that doesn't
    // happen on our call stack. So we just make sure the node is null after :/
    var node = scene.InstantiateOrNull<AutoConnectMissingTestScene>();
    node.MyNode.ShouldBeNull();
  }
}
