namespace Chickensoft.AutoInject.Tests.Subjects;

using Chickensoft.AutoInject;
using Godot;
using SuperNodes.Types;

// Provider nodes created to be used as test subjects.

[SuperNode(typeof(Provider))]
public partial class StringProvider : Node, IProvide<string> {
  public override partial void _Notification(int what);

  string IProvide<string>.Value() => Value;

  public bool OnProvidedCalled { get; private set; }
  public string Value { get; set; } = "";

  public void OnReady() => Provide();

  public void OnProvided() => OnProvidedCalled = true;
}

[SuperNode(typeof(Provider))]
public partial class IntProvider : Node, IProvide<int> {
  public override partial void _Notification(int what);

  int IProvide<int>.Value() => Value;

  public void OnReady() => Provide();

  public bool OnProvidedCalled { get; private set; }
  public int Value { get; set; }

  public void OnProvided() => OnProvidedCalled = true;
}
