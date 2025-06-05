namespace Chickensoft.AutoInject.Analyzers.Tests;

using System.Threading.Tasks;
using Chickensoft.AutoInject.Analyzers.Utils;
using Xunit;
using VerifyCS = Verifiers.CSharpAnalyzerVerifier<AutoInjectNotifyAnalyzer>;

public class AutoInjectNotifyAnalyzerTest {
  [Fact]
  public async Task ReportsMissingNotificationOverride() {
    var diagnosticID = Diagnostics
      .MissingAutoInjectNotificationOverrideDescriptor
      .Id;

    var testCode = $$"""
    using Godot;
    using Chickensoft.AutoInject;
    using Chickensoft.Introspection;

    [{|{{diagnosticID}}:Meta(typeof(IAutoNode))|}]
    class MyNode : Node
    {
    }
    """;

    await VerifyCS.VerifyAnalyzerAsync(testCode);
  }
}
