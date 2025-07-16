namespace Chickensoft.AutoInject.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Utils;

/// <summary>
/// When inheriting IProvide, the class must call this.Provide() somewhere in the setup.
/// This analyzer checks that the class does not forget to call this.Provide().
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AutoInjectProvideAnalyzer : DiagnosticAnalyzer {
  private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics =
    [Diagnostics.MissingAutoInjectProvideDescriptor];

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    _supportedDiagnostics;

  public override void Initialize(AnalysisContext context) {
    context.EnableConcurrentExecution();

    context.ConfigureGeneratedCodeAnalysis(
      GeneratedCodeAnalysisFlags.None
    );

    context.RegisterSyntaxNodeAction(
      AnalyzeClassDeclaration,
      SyntaxKind.ClassDeclaration
    );
  }

  private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context) {
    var classDeclaration = (ClassDeclarationSyntax)context.Node;

    if (classDeclaration.BaseList is null) {
      return;
    }

    var iProvideBaseType = classDeclaration
      .BaseList
      .Types
      .FirstOrDefault(
        type =>
          type.Type is GenericNameSyntax genericName
            && genericName.Identifier.ValueText == Constants.PROVIDER_INTERFACE_NAME
      );

    if (iProvideBaseType is null) {
      return;
    }

    // Check that Meta attribute has an AutoInject Provider type (ex: [Meta(typeof(IAutoNode))])
    var attributes = classDeclaration.AttributeLists.SelectMany(list => list.Attributes
      ).Where(attribute => attribute.Name.ToString() == Constants.META_ATTRIBUTE_NAME
         && attribute.ArgumentList?.Arguments.Any(arg =>
           arg.Expression is TypeOfExpressionSyntax { Type: IdentifierNameSyntax identifierName } &&
           Constants.ProviderMetaNames.Contains(identifierName.Identifier.ValueText)
         ) == true
      )
      .ToList();

    if (attributes.Count == 0) {
      return;
    }

    const string provideMethodName = "Provide";

    // Check if the class calls "this.Provide()" anywhere
    var hasProvide = classDeclaration
      .DescendantNodes()
      .OfType<InvocationExpressionSyntax>()
      .Any(invocation =>
        invocation.Expression is MemberAccessExpressionSyntax {
          Name.Identifier.ValueText: provideMethodName, Expression: ThisExpressionSyntax
        });

    if (hasProvide) {
      return;
    }

    // No provide call found, report the diagnostic
    context.ReportDiagnostic(
      Diagnostics.MissingAutoInjectProvide(
        attributes[0].GetLocation(),
        classDeclaration.Identifier.ValueText
      )
    );
  }
}
