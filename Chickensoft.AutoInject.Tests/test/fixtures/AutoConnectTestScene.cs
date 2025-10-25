namespace Chickensoft.AutoInject.Tests.Fixtures;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoConnect))]
public partial class AutoConnectTestScene : Node2D
{
  public override void _Notification(int what) => this.Notify(what);

  public static Node2D MyNode2D { get; } = new Node2D();

  [Node("Path/To/MyNode")]
  public INode2D MyNode { get; set; } = default!;

  [Node("Path/To/MyNode")]
  public Node2D MyNodeOriginal { get; set; } = default!;

  [Node]
  public INode2D MyUniqueNode { get; set; } = default!;

  [Node("%OtherUniqueName")]
  public INode2D DifferentName { get; set; } = default!;

#pragma warning disable IDE1006
  [Node]
  internal INode2D _my_unique_node { get; set; } = default!;

  [Node]
  public Node2D AlreadySetNode { get; set; } = MyNode2D;

  private Node2D _notGettableNodeField = default!;

  [Node]
  public Node2D NotGettableNode { set => _notGettableNodeField = value; }

  [Other]
  public INode2D SomeOtherNodeReference { get; set; } = default!;
}
