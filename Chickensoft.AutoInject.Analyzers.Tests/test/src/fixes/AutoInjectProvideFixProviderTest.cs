namespace Chickensoft.AutoInject.Analyzers.Tests;

using System.Threading.Tasks;
using Chickensoft.AutoInject.Analyzers.Utils;
using Xunit;
using VerifyCS = Verifiers.CSharpCodeFixVerifier<AutoInjectProvideAnalyzer, Fixes.AutoInjectProvideFixProvider>;

// It's nontrivial to get Introspection source generation to run during the
// test, so we fake some of the things that would ordinarily rely on it (e.g.,
// AutoInject types, some extension methods)
//
// TODO: We may need to split the code-fix provider into several separate
// providers - unclear if it's possible to test more than one fix from a single
// provider.
public class AutoInjectProvideFixProviderTest {
  [Fact]
  public async Task ReportsMissingNotificationOverride() {
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
      fixedCode.ReplaceLineEndings()
    );
  }
}
