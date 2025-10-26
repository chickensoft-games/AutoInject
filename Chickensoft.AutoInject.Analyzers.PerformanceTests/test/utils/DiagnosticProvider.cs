namespace Chickensoft.AutoInject.Analyzers.PerformanceTests.Utils;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

public class DiagnosticProvider : FixAllContext.DiagnosticProvider
{
  private readonly Project _project;
  private readonly IEnumerable<Diagnostic> _diagnostics;

  public DiagnosticProvider(
    Project project,
    IEnumerable<Diagnostic> diagnostics
  )
  {
    _project = project;
    _diagnostics = diagnostics;
  }

  public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(
    Project project,
    CancellationToken cancellationToken
  ) =>
    project == _project
      ? Task.FromResult(_diagnostics)
      : Task.FromResult((IEnumerable<Diagnostic>)[]);

  public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(
    Document document,
    CancellationToken cancellationToken
  ) =>
    Task.FromResult(
      _diagnostics
        .Where(
          entry => entry.Location.GetLineSpan().Path == document.FilePath
        )
    );

  public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(
    Project project,
    CancellationToken cancellationToken
  ) =>
    project == _project
      ? Task.FromResult(
          _diagnostics
            .Where(
              entry => !entry.Location.IsInSource
            )
        )
      : Task.FromResult((IEnumerable<Diagnostic>)[]);
}
