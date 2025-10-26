namespace Chickensoft.AutoInject.Tests.Fixtures;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoConnect))]
public partial class AutoConnectMissingTestScene : Node2D
{
  public override void _Notification(int what) => this.Notify(what);

  [Node("NonExistentNode")]
  public INode2D MyNode { get; set; } = default!;
}
