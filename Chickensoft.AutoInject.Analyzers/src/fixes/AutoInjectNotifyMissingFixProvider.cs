namespace Chickensoft.AutoInject.Analyzers.Fixes;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Utils;

[
  ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(AutoInjectNotifyMissingFixProvider)
  ),
  Shared
]
public class AutoInjectNotifyMissingFixProvider : CodeFixProvider {
  private static readonly InvocationExpressionSyntax _notifyInvocation =
    MethodModifier.ThisMemberCallExpression(Constants.NOTIFY_METHOD_NAME, []);
  private static readonly ImmutableArray<string> _fixableDiagnosticIds =
    [Diagnostics.MissingAutoInjectNotifyDescriptor.Id];

  public sealed override ImmutableArray<string> FixableDiagnosticIds =>
    _fixableDiagnosticIds;

  public sealed override FixAllProvider GetFixAllProvider() =>
    WellKnownFixAllProviders.BatchFixer;

  public sealed override async Task RegisterCodeFixesAsync(
      CodeFixContext context) {
    var root = await context.Document
      .GetSyntaxRootAsync(context.CancellationToken)
      .ConfigureAwait(false);
    if (root is null) {
      return;
    }

    var diagnostic = context.Diagnostics.First();
    var diagnosticSpan = diagnostic.Location.SourceSpan;

    // Find the type declaration identified by the diagnostic
    var typeDeclaration = root
      .FindToken(diagnosticSpan.Start)
      .Parent?
      .AncestorsAndSelf()
      .OfType<TypeDeclarationSyntax>().FirstOrDefault();
    if (typeDeclaration is null) {
      return;
    }

    context.RegisterCodeFix(
      CodeAction.Create(
        title: "Add \"this.Notify(what);\" to existing \"_Notification\" override",
        createChangedDocument: c =>
          AddAutoInjectNotifyCallAsync(context.Document, typeDeclaration, c),
        equivalenceKey: nameof(AutoInjectNotificationOverrideFixProvider)
      ),
      diagnostic
    );
  }

  private static async Task<Document> AddAutoInjectNotifyCallAsync(
    Document document,
    TypeDeclarationSyntax typeDeclaration,
    CancellationToken cancellationToken
  ) {
    // Find the method with the specified name and a single parameter of type int
    var methodAndParameter = typeDeclaration.Members
      .OfType<MethodDeclarationSyntax>()
      .Where(
        m =>
          m.Identifier.ValueText == Constants.NOTIFICATION_METHOD_NAME
            && m.ParameterList.Parameters.Count == 1
      )
      .Select(
        m => new {
          Method = m,
          Parameter = m
            .ParameterList
            .Parameters
            .FirstOrDefault(
              p =>
                p.Type is PredefinedTypeSyntax pts
                  && pts.Keyword.IsKind(SyntaxKind.IntKeyword)
            )
        }
      )
      .FirstOrDefault();

    var originalMethodNode = methodAndParameter?.Method;
    var parameterSyntax = methodAndParameter?.Parameter;

    if (originalMethodNode is null || parameterSyntax is null) {
      // Expected method not found or parameter is missing
      return document;
    }

    // Get the actual name of the parameter from the found method
    // It really should be "what", but this makes it more robust
    // to changes in the parameter name
    var actualParameterName = parameterSyntax.Identifier.ValueText;

    var statementToAdd = SyntaxFactory.ExpressionStatement(
        _notifyInvocation
        .WithArgumentList(
          SyntaxFactory.ArgumentList(
            SyntaxFactory.SingletonSeparatedList(
              SyntaxFactory.Argument(
                SyntaxFactory.IdentifierName(actualParameterName)
              )
            )
          )
        )
      );

    // Add the statement to the method body
    return await MethodModifier.AddStatementToMethodBodyAsync(
        document,
        originalMethodNode,
        statementToAdd,
        cancellationToken
    );
  }
}
