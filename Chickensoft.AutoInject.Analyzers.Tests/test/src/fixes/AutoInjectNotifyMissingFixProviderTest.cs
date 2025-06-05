namespace Chickensoft.AutoInject.Analyzers.Tests;

using System.Threading.Tasks;
using Chickensoft.AutoInject.Analyzers.Utils;
using Xunit;
using VerifyCS = Verifiers.CSharpCodeFixVerifier<AutoInjectNotifyAnalyzer, Fixes.AutoInjectNotifyMissingFixProvider>;

public class AutoInjectNotifyMissingFixProviderTest {
  [Fact]
  public async Task FixesMissingNotifyCallByAddingToOverride() {
    var diagnosticID = Diagnostics
      .MissingAutoInjectNotifyDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.AutoInject;
    using Chickensoft.Introspection;
    using Godot;

    [{|{{diagnosticID}}:Meta(typeof(IAutoNode))|}]
    partial class MyNode : Node
    {
        public override void _Notification(int what) { }
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.AutoInject;
    using Chickensoft.Introspection;
    using Godot;

    [Meta(typeof(IAutoNode))]
    partial class MyNode : Node
    {
        public override void _Notification(int what)
        {
            this.Notify(what);
        }
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings()
    );
  }
}
