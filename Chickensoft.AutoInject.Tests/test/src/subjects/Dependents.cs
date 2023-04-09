namespace Chickensoft.AutoInject.Tests.Subjects;

using Godot;
using SuperNodes.Types;

[SuperNode(typeof(Dependent))]
public partial class StringDependent : Node {
  [Dependency]
  public string MyDependency => DependOn<string>();

  public bool OnResolvedCalled { get; private set; }
  public string ResolvedValue { get; set; } = "";

  public override partial void _Notification(int what);

  public void OnReady() { }

  public void OnResolved() {
    OnResolvedCalled = true;
    ResolvedValue = MyDependency;
  }
}

[SuperNode(typeof(Dependent))]
public partial class StringDependentFallback : Node {
  [Dependency]
  public string MyDependency => DependOn(() => FallbackValue);

  public string FallbackValue { get; set; } = "";
  public bool OnResolvedCalled { get; private set; }
  public string ResolvedValue { get; set; } = "";

  public override partial void _Notification(int what);

  public void OnReady() { }

  public void OnResolved() {
    OnResolvedCalled = true;
    ResolvedValue = MyDependency;
  }
}

[SuperNode(typeof(Dependent))]
public partial class IntDependent : Node {
  [Dependency]
  public int MyDependency => DependOn<int>();

  public bool OnResolvedCalled { get; private set; }
  public int ResolvedValue { get; set; }

  public override partial void _Notification(int what);

  public void OnReady() { }

  public void OnResolved() {
    OnResolvedCalled = true;
    ResolvedValue = MyDependency;
  }
}
