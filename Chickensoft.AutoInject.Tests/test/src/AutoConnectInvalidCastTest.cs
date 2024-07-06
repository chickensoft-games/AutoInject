namespace Chickensoft.AutoInject.Tests;

using System;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.GoDotTest;
using Chickensoft.AutoInject.Tests.Fixtures;
using Godot;
using Moq;
using Shouldly;

public class AutoConnectInvalidCastTest(Node testScene) : TestClass(testScene) {
  [Test]
  public void ThrowsOnIncorrectNodeType() {
    var scene = GD.Load<PackedScene>(
      "res://test/fixtures/AutoConnectInvalidCastTestScene.tscn"
    );
    // AutoNode will actually throw an InvalidCastException
    // during the scene instantiation, but for whatever reason that doesn't
    // happen on our call stack. So we just make sure the node is null after :/
    var node = scene.Instantiate<AutoConnectInvalidCastTestScene>();
    node.Node.ShouldBeNull();
  }

  [Test]
  public void ThrowsIfFakedChildNodeIsWrongType() {
    var scene = new AutoConnectInvalidCastTestScene();
    scene.FakeNodeTree(new() { ["Node3D"] = new Mock<INode3D>().Object });

    Should.Throw<InvalidOperationException>(
      () => scene._Notification((int)Node.NotificationEnterTree)
    );
  }

  [Test]
  public void ThrowsIfNoNode() {
    var scene = new AutoConnectInvalidCastTestScene();
    Should.Throw<InvalidOperationException>(
      () => scene._Notification((int)Node.NotificationEnterTree)
    );
  }

  [Test]
  public void ThrowsIfTypeIsWrong() {
    var scene = new AutoConnectInvalidCastTestScene();

    var node = new Control {
      Name = "Node3D"
    };
    scene.AddChild(node);

    Should.Throw<InvalidOperationException>(
      () => scene._Notification((int)Node.NotificationEnterTree)
    );
  }
}
