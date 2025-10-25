namespace Chickensoft.AutoInject.Analyzers.Tests;

using System.Threading.Tasks;
using Chickensoft.AutoInject.Analyzers.Utils;
using Xunit;
using VerifyCS = Verifiers.CSharpCodeFixVerifier<AutoInjectNotificationOverrideMissingAnalyzer, Fixes.AutoInjectNotificationOverrideFixProvider>;

public class AutoInjectNotificationOverrideFixProviderTest
{
  [Fact]
  public async Task DoesNotOfferDiagnosticIfNotificationOverrideExists()
  {
    var testCode = $$"""
    using Chickensoft.AutoInject;
    using Chickensoft.Introspection;
    using Godot;

    [Meta(typeof(IAutoNode))]
    partial class MyNode : Node
    {
        public override void _Notification(int what) { }
    }
    """;

    await VerifyCS.VerifyAnalyzerAsync(
      testCode.ReplaceLineEndings()
    );
  }

  [Fact]
  public async Task FixesMissingNotificationOverrideByAddingOverrideWithNotify()
  {
    var diagnosticID = Diagnostics
      .MissingAutoInjectNotificationOverrideDescriptor
      .Id;

    var testCode = $$"""
    using Chickensoft.AutoInject;
    using Chickensoft.Introspection;
    using Godot;

    [{|{{diagnosticID}}:Meta(typeof(IAutoNode))|}]
    partial class MyNode : Node
    {
    }
    """;

    var fixedCode = $$"""
    using Chickensoft.AutoInject;
    using Chickensoft.Introspection;
    using Godot;

    [Meta(typeof(IAutoNode))]
    partial class MyNode : Node
    {
        public override void _Notification(int what) => this.Notify(what);
    }
    """;

    await VerifyCS.VerifyCodeFixAsync(
      testCode.ReplaceLineEndings(),
      fixedCode.ReplaceLineEndings()
    );
  }
}
