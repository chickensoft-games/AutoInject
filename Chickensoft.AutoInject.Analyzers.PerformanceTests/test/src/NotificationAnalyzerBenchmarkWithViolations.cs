namespace Chickensoft.AutoInject.Analyzers.PerformanceTests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Chickensoft.AutoInject.Analyzers.PerformanceTests.Utils;
using Microsoft.CodeAnalysis.Diagnostics;

public class NotificationAnalyzerBenchmarkWithViolations {
  private static CompilationWithAnalyzers _baselineCompilation = default!;
  private static CompilationWithAnalyzers _compilation = default!;

  [IterationSetup]
  public static void CreateEnvironment() {
    var sources = new List<(string name, string content)>();
    for (var i = 0; i < Constants.ANALYZER_SOURCES_COUNT; ++i) {
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

    var (compilation, options) = Compilations
      .CreateCompilation([.. sources], properties)
      .GetAwaiter()
      .GetResult();
    if (compilation is null) {
      throw new InvalidOperationException("Got null compilation");
    }
    _baselineCompilation = compilation.WithAnalyzers([new BaselineAnalyzer()], options);
    _compilation = compilation.WithAnalyzers([new AutoInjectNotificationOverrideMissingAnalyzer()], options);
  }

  [Benchmark(Baseline = true)]
  public async Task NotificationDiagnosticsBaseline() {
    var analysisResult = await _baselineCompilation.GetAnalysisResultAsync(CancellationToken.None);
    if (analysisResult.Analyzers.Length != 1) {
      throw new InvalidOperationException($"Analysis should have 1 analyzer (got {analysisResult.Analyzers.Length})");
    }
    if (analysisResult.CompilationDiagnostics.Count != 0) {
      throw new InvalidOperationException($"Baseline analysis should have 0 compiler diagnostics (got {analysisResult.CompilationDiagnostics.Count})");
    }
    var diagnostics = analysisResult.GetAllDiagnostics(analysisResult.Analyzers[0]);
    if (diagnostics.Length != 0) {
      throw new InvalidOperationException($"Baseline analysis should have 0 analyzer diagnostics (got {diagnostics.Length})");
    }
  }

  [Benchmark]
  public async Task NotificationDiagnostics() {
    var analysisResult = await _compilation.GetAnalysisResultAsync(CancellationToken.None);
    if (analysisResult.Analyzers.Length != 1) {
      throw new InvalidOperationException($"Analysis should have 1 analyzer (got {analysisResult.Analyzers.Length})");
    }
    if (analysisResult.CompilationDiagnostics.Count != 0) {
      throw new InvalidOperationException($"Baseline analysis should have 0 compiler diagnostics (got {analysisResult.CompilationDiagnostics.Count})");
    }
    var diagnostics = analysisResult.GetAllDiagnostics(analysisResult.Analyzers[0]);
    if (diagnostics.Length != Constants.ANALYZER_SOURCES_COUNT) {
      throw new InvalidOperationException($"Baseline analysis should have {Constants.ANALYZER_SOURCES_COUNT} analyzer diagnostics (got {diagnostics.Length})");
    }
  }
}
