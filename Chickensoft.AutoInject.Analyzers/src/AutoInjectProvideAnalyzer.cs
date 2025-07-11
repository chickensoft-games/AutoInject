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
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
    get;
  } = [Diagnostics.MissingAutoInjectProvideDescriptor];

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

    // Check that IProvide is implemented by the class, as these are the only classes that need to call Provide().
    var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
    var implementsIProvide = classSymbol?.AllInterfaces
      .Any(i => i.Name == Constants.PROVIDER_INTERFACE_NAME && i.IsGenericType) == true;

    if (!implementsIProvide) {
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
