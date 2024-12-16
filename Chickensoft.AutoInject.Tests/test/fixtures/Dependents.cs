namespace Chickensoft.AutoInject.Tests.Subjects;

using Chickensoft.Introspection;
using Godot;

[Meta(typeof(IAutoOn), typeof(IDependent))]
public partial class StringDependent : Node {
  public override void _Notification(int what) => this.Notify(what);

  [Dependency]
  public string MyDependency => this.DependOn<string>();

  public bool OnResolvedCalled { get; private set; }
  public string ResolvedValue { get; set; } = "";

  public void OnReady() { }

  public void OnResolved() {
    OnResolvedCalled = true;
    ResolvedValue = MyDependency;
  }
}

[Meta(typeof(IAutoOn), typeof(IDependent))]
public partial class FakedDependent : Node {
  public override void _Notification(int what) => this.Notify(what);

  [Dependency]
  public string MyDependency => this.DependOn(() => "fallback");

  public bool OnResolvedCalled { get; private set; }
  public string ResolvedValue { get; set; } = "";

  public void OnResolved() {
    OnResolvedCalled = true;
    ResolvedValue = MyDependency;
  }
}

[Meta(typeof(IAutoOn), typeof(IDependent))]
public partial class StringDependentFallback : Node {
  public override void _Notification(int what) => this.Notify(what);

  [Dependency]
  public string MyDependency => this.DependOn(() => FallbackValue);

  public string FallbackValue { get; set; } = "";
  public bool OnResolvedCalled { get; private set; }
  public string ResolvedValue { get; set; } = "";

  public void OnReady() { }

  public void OnResolved() {
    OnResolvedCalled = true;
    ResolvedValue = MyDependency;
  }
}

[Meta(typeof(IAutoOn), typeof(IDependent))]
public partial class ReferenceDependentFallback : Node {
  public override void _Notification(int what) => this.Notify(what);

  [Dependency]
  public object MyDependency => this.DependOn(() => FallbackValue);

  public object FallbackValue { get; set; } = new Resource();
  public bool OnResolvedCalled { get; private set; }
  public object ResolvedValue { get; set; } = null!;

  public void OnReady() { }

  public void OnResolved() {
    OnResolvedCalled = true;
    ResolvedValue = MyDependency;
  }
}

[Meta(typeof(IAutoOn), typeof(IDependent))]
public partial class IntDependent : Node {
  public override void _Notification(int what) => this.Notify(what);

  [Dependency]
  public int MyDependency => this.DependOn<int>();

  public bool OnResolvedCalled { get; private set; }
  public int ResolvedValue { get; set; }

  public void OnReady() { }

  public void OnResolved() {
    OnResolvedCalled = true;
    ResolvedValue = MyDependency;
  }
}

[Meta(typeof(IAutoOn), typeof(IDependent))]
public partial class MultiDependent : Node {
  public override void _Notification(int what) => this.Notify(what);

  [Dependency]
  public int IntDependency => this.DependOn<int>();

  [Dependency]
  public string StringDependency => this.DependOn<string>();

  public bool OnResolvedCalled { get; private set; }
  public int IntResolvedValue { get; set; }
  public string StringResolvedValue { get; set; } = null!;
  public bool ReadyCalled { get; set; }
  public void OnReady() => ReadyCalled = true;

  public void OnResolved() {
    OnResolvedCalled = true;
    IntResolvedValue = IntDependency;
    StringResolvedValue = StringDependency;
  }
}

[Meta(typeof(IAutoOn), typeof(IDependent))]
public partial class NoDependenciesDependent : Node {
  public override void _Notification(int what) => this.Notify(what);

  public bool OnResolvedCalled { get; private set; }

  public void OnResolved() => OnResolvedCalled = true;
}
