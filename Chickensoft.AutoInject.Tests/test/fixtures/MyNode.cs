namespace Chickensoft.AutoInject.Tests.Fixtures;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoConnect))]
public partial class MyNode : Node2D
{
  public override void _Notification(int what) => this.Notify(what);

  [Node("Path/To/SomeNode")]
  public INode2D SomeNode { get; set; } = default!;

  [Node] // Connects to "%MyUniqueNode" since no path was specified.
  public INode2D MyUniqueNode { get; set; } = default!;

  [Node("%OtherUniqueName")]
  public INode2D DifferentName { get; set; } = default!;

#pragma warning disable IDE1006
  [Node] // Connects to "%MyUniqueNode" since no path was specified.
  internal INode2D _my_unique_node { get; set; } = default!;
}
