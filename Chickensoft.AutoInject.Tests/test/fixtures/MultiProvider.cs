namespace Chickensoft.AutoInject.Tests.Fixtures;

using Chickensoft.AutoInject;
using Chickensoft.AutoInject.Tests.Subjects;
using Godot;
using SuperNodes.Types;

[SuperNode(typeof(Provider))]
public partial class MultiProvider : Node2D, IProvide<int>, IProvide<string> {
  public override partial void _Notification(int what);

  int IProvide<int>.Value() => IntValue;
  string IProvide<string>.Value() => StringValue;

  public MultiDependent Child { get; private set; } = default!;

  public override void _Ready() {
    Child = new MultiDependent();
    AddChild(Child);

    Provide();
  }

  public bool OnProvidedCalled { get; private set; }
  public int IntValue { get; set; }
  public string StringValue { get; set; } = "";

  public void OnProvided() => OnProvidedCalled = true;
}
