namespace Chickensoft.AutoInject.Analyzers.fixes;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Utils;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AutoInjectNotifyOverrideFixProvider)), Shared]
public class AutoInjectNotifyOverrideFixProvider : CodeFixProvider {
  public sealed override ImmutableArray<string> FixableDiagnosticIds =>
    [Diagnostics.MissingAutoInjectNotifyOverrideDescriptor.Id];

  public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

  public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
    var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
    if (root is null) {
      return;
    }

    var diagnostic = context.Diagnostics.First();
    var diagnosticSpan = diagnostic.Location.SourceSpan;

    // Find the type declaration identified by the diagnostic
    var typeDeclaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
      .OfType<TypeDeclarationSyntax>().FirstOrDefault();
    if (typeDeclaration is null) {
      return;
    }

    context.RegisterCodeFix(
      CodeAction.Create(
        title: "Add \"public override void _Notification(int what) => this.Notify(what);\" method",
        createChangedDocument: c => AddAutoInjectNotifyOverrideAsync(context.Document, typeDeclaration, c),
        equivalenceKey: nameof(AutoInjectNotifyOverrideFixProvider)),
      diagnostic);
  }

  private static async Task<Document> AddAutoInjectNotifyOverrideAsync(Document document,
    TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
  {

    var methodDeclaration = SyntaxFactory.MethodDeclaration(
        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
        "_Notification")
      .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
        SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
      .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(
        SyntaxFactory.Parameter(SyntaxFactory.Identifier("what"))
          .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword))))));

    var expressionBody = SyntaxFactory.InvocationExpression(
        SyntaxFactory.MemberAccessExpression(
          SyntaxKind.SimpleMemberAccessExpression,
          SyntaxFactory.ThisExpression(),
          SyntaxFactory.IdentifierName("Notify")))
      .WithArgumentList(SyntaxFactory.ArgumentList(
        SyntaxFactory.SingletonSeparatedList(
          SyntaxFactory.Argument(SyntaxFactory.IdentifierName("what")))));

    var arrowExpressionClause = SyntaxFactory.ArrowExpressionClause(expressionBody)
      .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

    methodDeclaration = methodDeclaration
      .WithExpressionBody(arrowExpressionClause)
      .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

    // Insert the new method at the beginning of the class members
    var existingMembers = typeDeclaration.Members;
    var newMembers = existingMembers.Insert(0, methodDeclaration);

    // Update the type declaration with the new list of members
    var newTypeDeclaration = typeDeclaration.WithMembers(newMembers);

    // Get the current root and replace the type declaration
    var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
    if (root is null) {
      return document;
    }

    var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);

    // Return the updated document.
    return document.WithSyntaxRoot(newRoot);
  }
}
