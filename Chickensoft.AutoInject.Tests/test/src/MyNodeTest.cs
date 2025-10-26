namespace Chickensoft.AutoInject.Tests;

using System.Threading.Tasks;
using Chickensoft.AutoInject.Tests.Fixtures;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.GoDotTest;
using Godot;
using GodotTestDriver;
using Moq;
using Shouldly;

#pragma warning disable CA1001
public class MyNodeTest(Node testScene) : TestClass(testScene)
{
  private Fixture _fixture = default!;
  private MyNode _scene = default!;

  private Mock<INode2D> _someNode = default!;
  private Mock<INode2D> _myUniqueNode = default!;
  private Mock<INode2D> _otherUniqueNode = default!;

  [Setup]
  public async Task Setup()
  {
    _fixture = new(TestScene.GetTree());

    _someNode = new();
    _myUniqueNode = new();
    _otherUniqueNode = new();

    _scene = new MyNode();
    _scene.FakeNodeTree(new()
    {
      ["Path/To/SomeNode"] = _someNode.Object,
      ["%MyUniqueNode"] = _myUniqueNode.Object,
      ["%OtherUniqueName"] = _otherUniqueNode.Object,
    });

    await _fixture.AddToRoot(_scene);
  }

  [Cleanup]
  public async Task Cleanup() => await _fixture.Cleanup();

  [Test]
  public void UsesFakeNodeTree()
  {
    // Making a new instance of a node without instantiating a scene doesn't
    // trigger NotificationSceneInstantiated, so if we want to make sure our
    // AutoNodes get hooked up and use the FakeNodeTree, we need to do it manually.
    _scene._Notification((int)Node.NotificationSceneInstantiated);

    _scene.SomeNode.ShouldBe(_someNode.Object);
    _scene.MyUniqueNode.ShouldBe(_myUniqueNode.Object);
    _scene.DifferentName.ShouldBe(_otherUniqueNode.Object);
    _scene._my_unique_node.ShouldBe(_myUniqueNode.Object);
  }
}
