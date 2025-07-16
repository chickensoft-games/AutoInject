namespace Chickensoft.AutoInject.Analyzers.PerformanceTests.Fixes;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Chickensoft.AutoInject.Analyzers.Fixes;
using Chickensoft.AutoInject.Analyzers.PerformanceTests.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

public class NotificationFixBenchmark {
  private static List<Document> _documents = [];
  private static AnalysisResult _analysisResult = default!;

  [IterationSetup]
  public static void CreateEnvironment() {
    var sources = new List<(string name, string content)>();
    for (var i = 0; i < Constants.FIX_SOURCES_COUNT; ++i) {
      var name = $"Node{i}";
      sources.Add(
        (
          name,
          $$"""
          [Meta(typeof(IAutoNode))]
          public class {{name}}
          {
          }
          """
        )
      );
    }
    var properties = new[]
    {
      ("build_property.TargetFramework", "net8"),
      ("build_property._SupportedPlatformList", "Linux,Windows,macOS"),
    };

    var (project, documents, options) = Compilations
      .CreateProject([.. sources], properties);
    _documents = documents;

    var compilation = Compilations
      .CreateCompilation(project)
      .GetAwaiter()
      .GetResult()
        ?? throw new InvalidOperationException("Got null compilation");
    var compilationWithAnalyzers = compilation.WithAnalyzers(
      [new AutoInjectNotificationOverrideMissingAnalyzer()],
      options
    );

    _analysisResult = compilationWithAnalyzers
      .GetAnalysisResultAsync(CancellationToken.None)
      .GetAwaiter()
      .GetResult();
  }

  [Benchmark(Baseline = true)]
  public async Task NotificationFixBaseline() {
    if (_analysisResult.Analyzers.Length != 1) {
      throw new InvalidOperationException($"Analysis should have 1 analyzer (got {_analysisResult.Analyzers.Length})");
    }
    if (_analysisResult.CompilationDiagnostics.Count != 0) {
      throw new InvalidOperationException($"Analysis should have 0 compiler diagnostics (got {_analysisResult.CompilationDiagnostics.Count})");
    }
    var diagnostics = _analysisResult.GetAllDiagnostics(_analysisResult.Analyzers[0]);
    if (diagnostics.Length != Constants.FIX_SOURCES_COUNT) {
      throw new InvalidOperationException($"Analysis should have {Constants.FIX_SOURCES_COUNT} analyzer diagnostics (got {diagnostics.Length})");
    }

    for (var i = 0; i < diagnostics.Length; ++i) {
      var actionBuilder = ImmutableArray.CreateBuilder<CodeAction>();
      var context = new CodeFixContext(_documents[i], diagnostics[i], (a, d) => actionBuilder.Add(a), CancellationToken.None);
      var codeFixProvider = new BaselineCodeFixProvider();
      await codeFixProvider.RegisterCodeFixesAsync(context);
      var actions = actionBuilder.ToImmutable();
      if (actions.Length != 0) {
        throw new InvalidOperationException($"Baseline code fix should have 0 actions (got {actions.Length})");
      }
    }
  }

  [Benchmark]
  public async Task NotificationFixDiagnostics() {
    if (_analysisResult.Analyzers.Length != 1) {
      throw new InvalidOperationException($"Analysis should have 1 analyzer (got {_analysisResult.Analyzers.Length})");
    }
    if (_analysisResult.CompilationDiagnostics.Count != 0) {
      throw new InvalidOperationException($"Analysis should have 0 compiler diagnostics (got {_analysisResult.CompilationDiagnostics.Count})");
    }
    var diagnostics = _analysisResult.GetAllDiagnostics(_analysisResult.Analyzers[0]);
    if (diagnostics.Length != Constants.FIX_SOURCES_COUNT) {
      throw new InvalidOperationException($"Analysis should have {Constants.FIX_SOURCES_COUNT} analyzer diagnostics (got {diagnostics.Length})");
    }

    for (var i = 0; i < diagnostics.Length; ++i) {
      var actionBuilder = ImmutableArray.CreateBuilder<CodeAction>();
      var context = new CodeFixContext(_documents[i], diagnostics[i], (a, d) => actionBuilder.Add(a), CancellationToken.None);
      var codeFixProvider = new AutoInjectNotificationOverrideFixProvider();
      await codeFixProvider.RegisterCodeFixesAsync(context);
      var actions = actionBuilder.ToImmutable();
      if (actions.Length != 1) {
        throw new InvalidOperationException($"Code fix should have 1 actions (got {actions.Length})");
      }
      var fixAllProvider = codeFixProvider.GetFixAllProvider();
      var fixAllContext = new FixAllContext(
        _documents[i],
        codeFixProvider,
        FixAllScope.Document,
        actions[0].EquivalenceKey,
        fixAllProvider.GetSupportedFixAllDiagnosticIds(codeFixProvider),
        new DiagnosticProvider(_documents[i].Project, diagnostics),
        CancellationToken.None
      );
      var action = await fixAllProvider.GetFixAsync(fixAllContext).ConfigureAwait(false);
      var operations = await action!.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
      var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
    }
  }
}
