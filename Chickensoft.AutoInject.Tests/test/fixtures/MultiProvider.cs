namespace Chickensoft.AutoInject.Tests.Fixtures;

using Chickensoft.AutoInject;
using Chickensoft.AutoInject.Tests.Subjects;
using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoOn), typeof(IProvider))]
public partial class MultiProvider : Node2D, IProvide<int>, IProvide<string> {
  public override void _Notification(int what) => this.Notify(what);

  int IProvide<int>.Value() => IntValue;
  string IProvide<string>.Value() => StringValue;

  public MultiDependent Child { get; private set; } = default!;

  public override void _Ready() {
    Child = new MultiDependent();
    AddChild(Child);

    this.Provide();
  }

  public bool OnProvidedCalled { get; private set; }
  public int IntValue { get; set; }
  public string StringValue { get; set; } = "";

  public void OnProvided() => OnProvidedCalled = true;
}
