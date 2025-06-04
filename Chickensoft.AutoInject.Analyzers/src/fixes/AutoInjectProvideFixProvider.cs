namespace Chickensoft.AutoInject.Analyzers.fixes;

using System;
using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Utils;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AutoInjectProvideFixProvider))]
[Shared]
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

    // Register code fixes for either creating or modifying methods that call `this.Provide()`.

    // Setup() Method Fixes
    RegisterMethodFixesAsync(
      context, typeDeclaration, diagnostic,
      "Setup",
      m => m.Identifier.Text == "Setup" &&
           m.Modifiers.Any(SyntaxKind.PublicKeyword),
      SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
    );

    // OnReady() Method Fixes
    RegisterMethodFixesAsync(
      context, typeDeclaration, diagnostic,
      "OnReady",
      m => m.Identifier.Text == "OnReady" &&
           m.Modifiers.Any(SyntaxKind.PublicKeyword),
      SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
    );

    // _Ready() Method Fixes
    RegisterMethodFixesAsync(
      context, typeDeclaration, diagnostic,
      "_Ready",
      m => m.Identifier.Text == "_Ready" &&
           m.Modifiers.Any(SyntaxKind.PublicKeyword) &&
           m.Modifiers.Any(SyntaxKind.OverrideKeyword),
      SyntaxFactory.TokenList(
        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
        SyntaxFactory.Token(SyntaxKind.OverrideKeyword)
      )
    );
  }

  /// <summary>
  ///   Registers code fixes for a method that either adds a call to an existing method if it exists
  ///   or creates a new method with the call.
  /// </summary>
  /// <param name="context">Code fix context</param>
  /// <param name="typeDeclaration">Type declaration of parent class</param>
  /// <param name="diagnostic">Diagnostic that triggered this</param>
  /// <param name="methodName">Method name to create</param>
  /// <param name="findPredicate">Predicate to find existing method</param>
  /// <param name="creationModifiers">Method modifiers to add to created method</param>
  private static void RegisterMethodFixesAsync(
    CodeFixContext context,
    TypeDeclarationSyntax typeDeclaration,
    Diagnostic diagnostic,
    string methodName,
    Func<MethodDeclarationSyntax, bool> findPredicate,
    IEnumerable<SyntaxToken> creationModifiers
  ) {
    var existingMethod = typeDeclaration.Members
      .OfType<MethodDeclarationSyntax>()
      .FirstOrDefault(findPredicate);

    if (existingMethod is not null) {
      // Method exists, offer to add a call to it
      context.RegisterCodeFix(
        CodeAction.Create(
          $"Add \"this.Provide();\" to existing \"{methodName}()\" method",
          c =>
            MethodModifier.AddCallToMethod(context.Document, typeDeclaration, existingMethod, "Provide", c),
          $"{nameof(AutoInjectProvideFixProvider)}_AddCallTo_{methodName}"),
        diagnostic);
    }
    else {
      // Method does not exist, offer to create it
      context.RegisterCodeFix(
        CodeAction.Create(
          $"Create \"{methodName}()\" method that calls \"this.Provide()\"",
          c => AddNewMethodAsync(
            context.Document, typeDeclaration, methodName, creationModifiers, c),
          $"{nameof(AutoInjectProvideFixProvider)}_CreateNew_{methodName}"),
        diagnostic);
    }
  }

  private static async Task<Document> AddNewMethodAsync(Document document, TypeDeclarationSyntax typeDeclaration,
    string identifier, IEnumerable<SyntaxToken> creationModifiers,
    CancellationToken cancellationToken) {
    // Create the new method
    var mewMethod = SyntaxFactory
      .MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), identifier)
      .WithModifiers(SyntaxFactory.TokenList(creationModifiers))
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
