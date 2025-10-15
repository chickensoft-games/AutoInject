namespace Chickensoft.AutoInject.Tests;

using System.Threading.Tasks;
using Chickensoft.GoDotTest;
using Chickensoft.AutoInject.Tests.Fixtures;
using Godot;
using GodotTestDriver;
using Shouldly;
using System;
using Chickensoft.Introspection;

public partial class AutoConnectTest(Node testScene) : TestClass(testScene) {
  private Fixture _fixture = default!;
  private AutoConnectTestScene _scene = default!;

  [Meta(typeof(IAutoConnect))]
  public partial class NotAGodotNode { }

  [Setup]
  public async Task Setup() {
    _fixture = new Fixture(TestScene.GetTree());
    _scene = await _fixture.LoadAndAddScene<AutoConnectTestScene>();
  }

  [Cleanup]
  public async Task Cleanup() => await _fixture.Cleanup();

  [Test]
  public void ConnectsNodesCorrectlyWhenInstantiated() {
    _scene.MyNode.ShouldNotBeNull();
    _scene.MyNodeOriginal.ShouldNotBeNull();
    _scene.MyUniqueNode.ShouldNotBeNull();
    _scene.DifferentName.ShouldNotBeNull();
    _scene._my_unique_node.ShouldNotBeNull();
    _scene.SomeOtherNodeReference.ShouldBeNull();
  }

  [Test]
  public void NonAutoConnectNodeThrows() {
    var node = new Node();
    Should.Throw<InvalidOperationException>(() => node.FakeNodeTree(null));
  }

  [Test]
  public void FakeNodesDoesNothingIfGivenNull() {
    IAutoConnect node = new AutoConnectTestScene();
    Should.NotThrow(() => node.FakeNodes = null);
  }

  [Test]
  public void AddStateIfNeededDoesNothingIfNotAGodotNode() {
    IAutoConnect node = new NotAGodotNode();
    Should.NotThrow(() => node._AddStateIfNeeded());
  }

  [Test]
  public void DoesNotOverwriteNonNullProperties() =>
    _scene.AlreadySetNode.ShouldBeSameAs(AutoConnectTestScene.MyNode2D);
}
