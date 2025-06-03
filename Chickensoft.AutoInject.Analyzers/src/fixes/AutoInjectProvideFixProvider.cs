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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AutoInjectProvideFixProvider)), Shared]
public class AutoInjectProvideFixProvider : CodeFixProvider {
  public sealed override ImmutableArray<string> FixableDiagnosticIds =>
    [Diagnostics.MissingAutoInjectProvideDescriptor.Id];

  public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

  public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
    var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
    if (root is null) {
      return;
    }

    var diagnostic = context.Diagnostics.First();
    var diagnosticSpan = diagnostic.Location.SourceSpan;

    // Find the type declaration identified by the diagnostic.
    var typeDeclaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
      .OfType<TypeDeclarationSyntax>().FirstOrDefault();
    if (typeDeclaration is null) {
      return;
    }

    var hasSetupMethod = typeDeclaration.Members
      .OfType<MethodDeclarationSyntax>()
      .Any(m => m.Identifier.Text == "Setup" && m.Modifiers.Any(SyntaxKind.PublicKeyword));

    var hasOnReadyMethod = typeDeclaration.Members
      .OfType<MethodDeclarationSyntax>()
      .Any(m => m.Identifier.Text == "OnReady" && m.Modifiers.Any(SyntaxKind.PublicKeyword));

    var hasOnReadyOverride = typeDeclaration.Members
      .OfType<MethodDeclarationSyntax>()
      .Any(m => m.Identifier.Text == "_Ready" && m.Modifiers.Any(SyntaxKind.PublicKeyword) &&
                m.Modifiers.Any(SyntaxKind.OverrideKeyword));

    // If they have Setup(), suggest adding this.Provide() at the end of it.
    if (hasSetupMethod) {
      var setupMethod = typeDeclaration.Members
        .OfType<MethodDeclarationSyntax>()
        .First(m => m.Identifier.Text == "Setup" && m.Modifiers.Any(SyntaxKind.PublicKeyword));

      context.RegisterCodeFix(
        CodeAction.Create(
          title: "Add \"this.Provide();\" to existing \"Setup()\" method",
          createChangedDocument: c => MethodModifier.AddCallToMethod(context.Document, typeDeclaration, setupMethod, "Provide", c),
          equivalenceKey: nameof(AutoInjectProvideFixProvider)),
        diagnostic);
    } else {
      // If they don't have Setup(), suggest creating one with this.Provide() in it.
      context.RegisterCodeFix(
        CodeAction.Create(
          title: "Create \"Setup()\" method that calls \"this.Provide();\"",
          createChangedDocument: c => AddNewMethodAsync(context.Document, typeDeclaration, "Setup", c),
          equivalenceKey: nameof(AutoInjectProvideFixProvider)),
        diagnostic);
    }

    // If they have OnReady(), suggest adding this.Provide() at the end of it.
    if (hasOnReadyMethod) {
      var onReadyMethod = typeDeclaration.Members
        .OfType<MethodDeclarationSyntax>()
        .First(m => m.Identifier.Text == "OnReady" && m.Modifiers.Any(SyntaxKind.PublicKeyword));

      context.RegisterCodeFix(
        CodeAction.Create(
          title: "Add \"this.Provide();\" to existing \"OnReady()\" method",
          createChangedDocument: c => MethodModifier.AddCallToMethod(context.Document, typeDeclaration, onReadyMethod, "Provide", c),
          equivalenceKey: nameof(AutoInjectProvideFixProvider)),
        diagnostic);
    } else {
      // If they don't have OnReady(), suggest creating one with this.Provide() in it.
      context.RegisterCodeFix(
        CodeAction.Create(
          title: "Create \"OnReady()\" method that calls \"this.Provide();\"",
          createChangedDocument: c => AddNewMethodAsync(context.Document, typeDeclaration, "OnReady", c),
          equivalenceKey: nameof(AutoInjectProvideFixProvider)),
        diagnostic);
    }

    // If they have _Ready(), suggest adding this.Provide() at the end of it.
    if (hasOnReadyOverride) {
      var onReadyOverrideMethod = typeDeclaration.Members
        .OfType<MethodDeclarationSyntax>()
        .First(m => m.Identifier.Text == "_Ready" && m.Modifiers.Any(SyntaxKind.PublicKeyword) &&
                    m.Modifiers.Any(SyntaxKind.OverrideKeyword));

      context.RegisterCodeFix(
        CodeAction.Create(
          title: "Add \"this.Provide();\" to existing \"_Ready()\" method",
          createChangedDocument: c => MethodModifier.AddCallToMethod(context.Document, typeDeclaration, onReadyOverrideMethod, "Provide", c),
          equivalenceKey: nameof(AutoInjectProvideFixProvider)),
        diagnostic);
    }
  }

  private static async Task<Document> AddNewMethodAsync(Document document, TypeDeclarationSyntax typeDeclaration, string identifier,
    CancellationToken cancellationToken) {

    // Create the new method
    var mewMethod = SyntaxFactory
      .MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), identifier)
      .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
      .WithBody(SyntaxFactory.Block(
        SyntaxFactory.SingletonList(
          SyntaxFactory.ParseStatement(Constants.PROVIDE_NEW_METHOD_BODY)
            .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
        )
      ));

    // Add the new method to the class
    var newTypeDeclaration = typeDeclaration.AddMembers(mewMethod);
    // Replace the old type declaration with the new one
    var root = await document.GetSyntaxRootAsync(cancellationToken);
    if (root is null) {
      return document;
    }

    var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);
    // Return the updated document
    return document.WithSyntaxRoot(newRoot);
  }
}
