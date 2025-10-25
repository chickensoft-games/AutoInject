namespace Chickensoft.AutoInject.Analyzers.Tests.Verifiers;

using System.Threading;
using System.Threading.Tasks;
using Chickensoft.AutoInject.Analyzers.Tests.Util;
using Godot;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Design",
  "CA1000: Do not declare static members on generic types",
  Justification = "CA1000 prefers no generic arguments, but either method or class needs them here"
)]
public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
{
  /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic()"/>
  public static DiagnosticResult Diagnostic() =>
      CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>
        .Diagnostic();

  /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(string)"/>
  public static DiagnosticResult Diagnostic(string diagnosticId) =>
      CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>
        .Diagnostic(diagnosticId);

  /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
  public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) =>
      CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>
        .Diagnostic(descriptor);

  public static Test CreateTest(string source, string? fixedSource = null)
  {
    var test = new Test
    {
      TestCode = source,
    };

    if (fixedSource is not null)
    {
      test.FixedCode = fixedSource;
    }

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

  /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
  public static async Task VerifyAnalyzerAsync(
      string source,
      params DiagnosticResult[] expected)
  {
    var test = CreateTest(source);

    test.ExpectedDiagnostics.AddRange(expected);
    await test.RunAsync(CancellationToken.None);
  }

  /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, string)"/>
  public static async Task VerifyCodeFixAsync(
      string source,
      string fixedSource,
      string? codeFixEquivalenceKey = null) =>
    await VerifyCodeFixAsync(
      source,
      DiagnosticResult.EmptyDiagnosticResults,
      fixedSource,
      codeFixEquivalenceKey
    );

  /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult, string)"/>
  public static async Task VerifyCodeFixAsync(
      string source,
      DiagnosticResult expected,
      string fixedSource,
      string? codeFixEquivalenceKey = null) =>
    await VerifyCodeFixAsync(
      source,
      [expected],
      fixedSource,
      codeFixEquivalenceKey
    );

  /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult[], string)"/>
  public static async Task VerifyCodeFixAsync(
      string source,
      DiagnosticResult[] expected,
      string fixedSource,
      string? codeFixEquivalenceKey)
  {
    var test = CreateTest(source, fixedSource);

    if (codeFixEquivalenceKey is not null)
    {
      test.CodeActionEquivalenceKey = codeFixEquivalenceKey;
    }

    test.ExpectedDiagnostics.AddRange(expected);
    await test.RunAsync(CancellationToken.None);
  }
}
