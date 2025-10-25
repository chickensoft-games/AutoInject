namespace Chickensoft.AutoInject.Analyzers.Tests.Verifiers;

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

public static partial class CSharpAnalyzerVerifier<TAnalyzer>
       where TAnalyzer : DiagnosticAnalyzer, new()
{
  public class Test : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
  {
    public Test()
    {
      SolutionTransforms.Add(
        (solution, projectId) =>
        {
          var project = solution.GetProject(projectId);
          if (project is null)
          {
            return solution;
          }
          var compilationOptions = project.CompilationOptions;
          if (compilationOptions is null)
          {
            return solution;
          }
          compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
              compilationOptions
                .SpecificDiagnosticOptions
                .SetItems(CSharpVerifierHelper.NullableWarnings)
            );
          solution = solution.WithProjectCompilationOptions(
            projectId,
            compilationOptions
          );
          return solution;
        }
      );
    }
  }
}
