namespace Chickensoft.AutoInject.Tests;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Chickensoft.AutoInject.Tests.Fixtures;
using Chickensoft.GoDotTest;
using Chickensoft.Introspection;
using Godot;
using GodotTestDriver;
using Shouldly;

// Future work: This test suite still has memory leak issues.

public partial class AutoConnectTest(Node testScene) : TestClass(testScene)
{
  private Fixture _fixture = default!;
  private AutoConnectTestScene _scene = default!;

  [Meta(typeof(IAutoConnect))]
  public partial class NotAGodotNode;

  [Setup]
  public async Task Setup()
  {
    _fixture = new Fixture(TestScene.GetTree());
    _scene = await _fixture.LoadAndAddScene<AutoConnectTestScene>();
  }

  [Cleanup]
  public async Task Cleanup()
  {
    await _fixture.Cleanup();
    foreach (var child in _scene.GetChildren())
    {
      child?.QueueFree();
    }
    _scene?.QueueFree();
  }

  [Test]
  public void ConnectsNodesCorrectlyWhenInstantiated()
  {
    _scene.MyNode.ShouldNotBeNull();
    _scene.MyNodeOriginal.ShouldNotBeNull();
    _scene.MyUniqueNode.ShouldNotBeNull();
    _scene.DifferentName.ShouldNotBeNull();
    _scene._my_unique_node.ShouldNotBeNull();
    _scene.SomeOtherNodeReference.ShouldBeNull();
  }

  [Test]
  public void NonAutoConnectNodeThrows()
  {
    var node = new Node();
    Should.Throw<InvalidOperationException>(() => node.FakeNodeTree(null));

    node.QueueFree();
  }

  [Test]
  public void FakeNodesDoesNothingIfGivenNull()
  {
    var node = new AutoConnectTestScene();
    Should.NotThrow(() => ((IAutoConnect)node).FakeNodes = null);

    node.QueueFree();
  }

  [
    SuppressMessage(
      "Performance",
      "CA1859",
      Justification = "Testing the interface"
    )
  ]
  [Test]
  public void AddStateIfNeededDoesNothingIfNotAGodotNode()
  {
    IAutoConnect node = new NotAGodotNode();
    Should.NotThrow(() => node._AddStateIfNeeded());
  }

  [Test]
  public void DoesNotOverwriteNonNullProperties() =>
    _scene.AlreadySetNode.ShouldBeSameAs(AutoConnectTestScene.MyNode2D);
}
