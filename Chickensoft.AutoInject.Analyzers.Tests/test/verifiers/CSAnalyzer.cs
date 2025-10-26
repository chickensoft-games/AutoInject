namespace Chickensoft.AutoInject.Analyzers.Tests.Verifiers;

using System.Threading;
using System.Threading.Tasks;
using Chickensoft.AutoInject;
using Chickensoft.AutoInject.Analyzers.Tests.Util;
using Godot;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Design",
  "CA1000: Do not declare static members on generic types",
  Justification = "CA1000 prefers no generic arguments, but either method or class needs them here"
)]
public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
  /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()"/>
  public static DiagnosticResult Diagnostic()
      => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic();

  /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)"/>
  public static DiagnosticResult Diagnostic(string diagnosticId)
      => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>
        .Diagnostic(diagnosticId);

  /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
  public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
      => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>
        .Diagnostic(descriptor);

  public static Test CreateTest(string source)
  {
    var test = new Test
    {
      TestCode = source,
    };

    var autoInjectAssemblyPath =
      AssemblyHelper.GetAssemblyPath(typeof(IAutoNode));
    var introspectionAssemblyPath =
      AssemblyHelper.GetAssemblyPath(typeof(Introspection.MetaAttribute));
    var godotAssemblyPath =
      AssemblyHelper.GetAssemblyPath(typeof(Node));

    test.ReferenceAssemblies = ReferenceAssemblies.Net.Net80
      .AddAssemblies(
        [
          autoInjectAssemblyPath,
          introspectionAssemblyPath,
          godotAssemblyPath,
        ]
      );

    return test;
  }

  /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
  public static async Task VerifyAnalyzerAsync(
      string source,
      params DiagnosticResult[] expected)
  {
    var test = CreateTest(source);

    test.ExpectedDiagnostics.AddRange(expected);
    await test.RunAsync(CancellationToken.None);
  }
}
