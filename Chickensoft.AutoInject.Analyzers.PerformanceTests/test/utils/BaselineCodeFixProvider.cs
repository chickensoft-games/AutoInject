namespace Chickensoft.AutoInject.Analyzers.PerformanceTests.Utils;

using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BaselineCodeFixProvider))]
public class BaselineCodeFixProvider : CodeFixProvider {
  private static readonly ImmutableArray<string> _diagnosticIds = [];

  public override FixAllProvider GetFixAllProvider() =>
    WellKnownFixAllProviders.BatchFixer;

  public override ImmutableArray<string> FixableDiagnosticIds => _diagnosticIds;

  public override Task RegisterCodeFixesAsync(CodeFixContext context) =>
    Task.CompletedTask;
}
