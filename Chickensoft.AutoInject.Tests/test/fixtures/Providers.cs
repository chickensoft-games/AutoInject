namespace Chickensoft.AutoInject.Tests.Subjects;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

// Provider nodes created to be used as test subjects.

[Meta(typeof(IAutoOn), typeof(IProvider))]
public partial class StringProvider : Node, IProvide<string>
{
  public override void _Notification(int what) => this.Notify(what);

  string IProvide<string>.Value() => Value;

  public bool OnProvidedCalled { get; private set; }
  public string Value { get; set; } = "";

  public void OnReady() => this.Provide();

  public void OnProvided() => OnProvidedCalled = true;
}

[Meta(typeof(IAutoOn), typeof(IProvider))]
public partial class IntProvider : Node, IProvide<int>
{
  public override void _Notification(int what) => this.Notify(what);

  int IProvide<int>.Value() => Value;

  public void OnReady() => this.Provide();

  public bool OnProvidedCalled { get; private set; }
  public int Value { get; set; }

  public void OnProvided() => OnProvidedCalled = true;
}
