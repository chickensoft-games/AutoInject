namespace Chickensoft.AutoInject.Tests.Fixtures;

using Chickensoft.Introspection;
using Chickensoft.AutoInject;
using Godot;

[Meta(typeof(IAutoInit))]
public partial class AutoInitTestNode : Node2D {
  public override void _Notification(int what) => this.Notify(what);

  public bool SetupCalled { get; set; }

  public void Initialize() => SetupCalled = true;
}

[Meta(typeof(IAutoInit))]
public partial class AutoInitTestNodeNoImplementation : Node2D {
  public override void _Notification(int what) => this.Notify(what);
}
