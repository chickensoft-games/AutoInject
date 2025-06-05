namespace Chickensoft.AutoInject.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Chickensoft.AutoInject.Analyzers.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AutoInjectNotificationOverrideMissingAnalyzer : DiagnosticAnalyzer {
  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
    get;
  } = [Diagnostics.MissingAutoInjectNotificationOverrideDescriptor];

  public override void Initialize(AnalysisContext context) {
    context.EnableConcurrentExecution();

    context.ConfigureGeneratedCodeAnalysis(
      GeneratedCodeAnalysisFlags.Analyze |
      GeneratedCodeAnalysisFlags.ReportDiagnostics
    );

    context.RegisterSyntaxNodeAction(
      AnalyzeClassDeclaration,
      SyntaxKind.ClassDeclaration
    );
  }

  private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context) {
    var classDeclaration = (ClassDeclarationSyntax)context.Node;

    var attributes = classDeclaration.AttributeLists.SelectMany(list => list.Attributes
    ).Where(attribute => attribute.Name.ToString() == Constants.META_ATTRIBUTE_NAME
       // Check that Meta attribute has an AutoInject type (ex: [Meta(typeof(IAutoNode))])
       && attribute.ArgumentList?.Arguments.Any(arg =>
          arg.Expression is TypeOfExpressionSyntax { Type: IdentifierNameSyntax identifierName } &&
         Constants.AutoInjectTypeNames.Contains(identifierName.Identifier.ValueText)
       ) == true
    )
    .ToList();

    if (attributes.Count == 0) {
      return;
    }

    // Check if the class has a _Notification override method.
    var hasNotificationOverride = classDeclaration
      .Members
      .OfType<MethodDeclarationSyntax>()
      .Any(method =>
        method.Identifier.ValueText == "_Notification" &&
        method.Modifiers.Any(SyntaxKind.OverrideKeyword) &&
        method.ParameterList.Parameters.Count == 1
      );

    if (!hasNotificationOverride) {
      // Report missing Notify call, _Notification override already exists.
      context.ReportDiagnostic(
        Diagnostics.MissingAutoInjectNotificationOverride(
          attributes[0].GetLocation(),
          classDeclaration.Identifier.ValueText
        )
      );
    }
  }
}
