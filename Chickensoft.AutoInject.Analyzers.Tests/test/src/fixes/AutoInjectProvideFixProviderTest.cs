namespace Chickensoft.AutoInject.Analyzers.Tests;

using System.Threading.Tasks;
using Chickensoft.AutoInject.Analyzers.Fixes;
using Chickensoft.AutoInject.Analyzers.Utils;
using Xunit;
using VerifyCS = Verifiers.CSharpCodeFixVerifier<AutoInjectProvideAnalyzer, Fixes.AutoInjectProvideFixProvider>;

// It's nontrivial to get Introspection source generation to run during the
// test, so we fake some of the things that would ordinarily rely on it (e.g.,
// AutoInject types, some extension methods)
public class AutoInjectProvideFixProviderTest
{
  #region NoDiagnostic
  [Fact]
  public async Task DoesNotOfferDiagnosticIfProvideIsCalled()
  {
    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void SomeMethod() => this.Provide();
    }
    """;

    await VerifyCS.VerifyAnalyzerAsync(testCode);
  }
  #endregion NoDiagnostic

  #region AddingMethods
  [Fact]
  public async Task FixesMissingProvideCallByAddingSetup()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);

        public void Setup()
        {
            // Call the this.Provide() method once your dependencies have been initialized.
            this.Provide();
        }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.SETUP_METHOD_NAME,
        false
      )
    );
  }

  [Fact]
  public async Task FixesMissingProvideCallByAddingOnReady()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);

        public void OnReady()
        {
            // Call the this.Provide() method once your dependencies have been initialized.
            this.Provide();
        }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.ONREADY_METHOD_NAME,
        false
      )
    );
  }

  [Fact]
  public async Task FixesMissingProvideCallByAddingReadyOverride()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);

        public override void _Ready()
        {
            // Call the this.Provide() method once your dependencies have been initialized.
            this.Provide();
        }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.READY_METHOD_NAME,
        false
      )
    );
  }
  #endregion AddingMethods

  #region ImprovingExistingMethods
  [Fact]
  public async Task FixesMissingProvideCallByImprovingExistingEmptySetup()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void Setup() { }
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void Setup()
        {
            this.Provide();
        }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.SETUP_METHOD_NAME,
        true
      )
    );
  }

  [Fact]
  public async Task FixesMissingProvideCallByImprovingExistingNonEmptySetup()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void Setup()
        {
            DoSomething(); // do something
        }
        public void DoSomething() { }
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void Setup()
        {
            DoSomething(); // do something
            this.Provide();
        }
        public void DoSomething() { }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.SETUP_METHOD_NAME,
        true
      )
    );
  }

  [Fact]
  public async Task FixesMissingProvideCallByImprovingExistingExpressionBodiedSetup()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void Setup() => DoSomething(); // do something
        public void DoSomething() { }
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void Setup()
        {
            DoSomething(); // do something
            this.Provide();
        }

        public void DoSomething() { }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.SETUP_METHOD_NAME,
        true
      )
    );
  }

  [Fact]
  public async Task FixesMissingProvideCallByImprovingExistingEmptyOnReady()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void OnReady() { }
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void OnReady()
        {
            this.Provide();
        }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.ONREADY_METHOD_NAME,
        true
      )
    );
  }

  [Fact]
  public async Task FixesMissingProvideCallByImprovingExistingNonEmptyOnReady()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void OnReady() {
            DoSomething(); // do something
        }
        public void DoSomething() { }
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void OnReady()
        {
            DoSomething(); // do something
            this.Provide();
        }
        public void DoSomething() { }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.ONREADY_METHOD_NAME,
        true
      )
    );
  }

  [Fact]
  public async Task FixesMissingProvideCallByImprovingExistingExpressionBodiedOnReady()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void OnReady() => DoSomething(); // do something
        public void DoSomething() { }
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public void OnReady()
        {
            DoSomething(); // do something
            this.Provide();
        }

        public void DoSomething() { }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.ONREADY_METHOD_NAME,
        true
      )
    );
  }

  [Fact]
  public async Task FixesMissingProvideCallByImprovingExistingEmptyReadyOverride()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public override void _Ready() { }
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public override void _Ready()
        {
            this.Provide();
        }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.READY_METHOD_NAME,
        true
      )
    );
  }

  [Fact]
  public async Task FixesMissingProvideCallByImprovingExistingNonEmptyReadyOverride()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public override void _Ready()
        {
            DoSomething(); // do something
        }
        public void DoSomething() { }
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public override void _Ready()
        {
            DoSomething(); // do something
            this.Provide();
        }
        public void DoSomething() { }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.READY_METHOD_NAME,
        true
      )
    );
  }

  [Fact]
  public async Task FixesMissingProvideCallByImprovingExistingExpressionBodiedReadyOverride()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectProvideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [{|{{diagnosticID}}:Meta(typeof(IProvider))|}]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public override void _Ready() => DoSomething(); // do something
        public void DoSomething() { }
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.Introspection;
    using Godot;

    interface IProvide<T>
    {
        T Value();
    }

    interface IProvider;

    public static class MyNodeExtensions
    {
        public static void Notify(this MyNode node, int what) { }
        public static void Provide(this MyNode node) { }
    }

    class Provision;

    [Meta(typeof(IProvider))]
    public partial class MyNode : Node, IProvide<Provision>
    {
        Provision IProvide<Provision>.Value() => new Provision();
        public override void _Notification(int what) => this.Notify(what);
        public override void _Ready()
        {
            DoSomething(); // do something
            this.Provide();
        }

        public void DoSomething() { }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings(),
      AutoInjectProvideFixProvider.GetCodeFixEquivalenceKey(
        Constants.READY_METHOD_NAME,
        true
      )
    );
  }
  #endregion ImprovingExistingMethods
}
