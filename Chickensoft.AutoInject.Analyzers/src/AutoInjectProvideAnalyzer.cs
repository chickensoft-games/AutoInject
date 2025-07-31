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
// disabling RS1038 is necessary no matter how much the analyzer says it's not
#pragma warning disable IDE0079
// we're only using Workspaces in the code fixes, not the analyzers
#pragma warning disable RS1038
[DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1038
#pragma warning restore IDE0079
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

  private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context) {
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
    var attribute = AnalyzerTools.GetAutoInjectMetaAttribute(
      classDeclaration,
      Constants.ProviderMetaNames.Contains
    );

    if (attribute is null) {
      return;
    }

    // Check if the class calls "this.Provide()" anywhere
    var hasProvide = AnalyzerTools.HasThisCall(
      classDeclaration,
      Constants.PROVIDE_METHOD_NAME
    );

    if (hasProvide) {
      return;
    }

    // No provide call found, report the diagnostic
    context.ReportDiagnostic(
      Diagnostics.MissingAutoInjectProvide(
        attribute.GetLocation(),
        classDeclaration.Identifier.ValueText
      )
    );
  }
}
