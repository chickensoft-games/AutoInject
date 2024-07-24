namespace Chickensoft.AutoInject.Tests.Fixtures;

using Chickensoft.Introspection;
using Chickensoft.AutoInject;
using Godot;

[Meta(typeof(IAutoInit))]
public partial class AutoInitTestNode : Node2D {
  public override void _Notification(int what) => this.Notify(what);

  public int Called { get; set; }

  public void Initialize() => Called++;
}

[Meta(typeof(IAutoNode))]
public partial class AutoInitTestAutoNode : Node2D {
  public override void _Notification(int what) => this.Notify(what);

  public int Called { get; set; }

  public void Initialize() => Called++;
}

[Meta(typeof(IAutoInit))]
public partial class AutoInitTestNodeNoImplementation : Node2D {
  public override void _Notification(int what) => this.Notify(what);
}
