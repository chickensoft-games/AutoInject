namespace Chickensoft.AutoInject.Tests.Fixtures;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoConnect))]
public partial class AutoConnectInvalidCastTestScene : Node2D
{
  public override void _Notification(int what) => this.Notify(what);

  [Node("Node3D")]
  public INode2D Node { get; set; } = default!;
}
