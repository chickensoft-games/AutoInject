namespace Chickensoft.AutoInject.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using Chickensoft.AutoInject.Analyzers.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// disabling RS1038 is necessary no matter how much the analyzer says it's not
#pragma warning disable IDE0079
// we're only using Workspaces in the code fixes, not the analyzers
#pragma warning disable RS1038
[DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1038
#pragma warning restore IDE0079
public class AutoInjectNotificationOverrideMissingAnalyzer : DiagnosticAnalyzer {
  private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics =
    [Diagnostics.MissingAutoInjectNotificationOverrideDescriptor];

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

  private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context) {
    var classDeclaration = (ClassDeclarationSyntax)context.Node;

    var attribute = AnalyzerTools.GetAutoInjectMetaAttribute(
      classDeclaration,
      Constants.AutoInjectMetaNames.Contains
    );

    if (attribute is null) {
      return;
    }

    // Check if the class has a _Notification override method.
    var notificationOverride = AnalyzerTools.GetMethodOverride(
      classDeclaration,
      Constants.NOTIFICATION_METHOD_NAME
    );

    if (notificationOverride is null) {
      // Report missing _Notification override.
      context.ReportDiagnostic(
        Diagnostics.MissingAutoInjectNotificationOverride(
          attribute.GetLocation(),
          classDeclaration.Identifier.ValueText
        )
      );
    }
  }
}
