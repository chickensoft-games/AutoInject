namespace Chickensoft.AutoInject.Analyzers.Fixes;

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
using Microsoft.CodeAnalysis.Editing;
using Utils;

[
  ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(AutoInjectProvideFixProvider)
  )
]
[Shared]
public class AutoInjectProvideFixProvider : CodeFixProvider
{
  private static readonly SyntaxToken _publicKeyword =
    SyntaxFactory.Token(SyntaxKind.PublicKeyword);
  private static readonly SyntaxToken _overrideKeyword =
    SyntaxFactory.Token(SyntaxKind.OverrideKeyword);
  private static readonly MethodDeclarationSyntax _setupMethodDeclaration =
    NewMethod(Constants.SETUP_METHOD_NAME, [_publicKeyword]);
  private static readonly MethodDeclarationSyntax _onReadyMethodDeclaration =
    NewMethod(Constants.ONREADY_METHOD_NAME, [_publicKeyword]);
  private static readonly MethodDeclarationSyntax _readyMethodDeclaration =
    NewMethod(
      Constants.READY_METHOD_NAME,
      [_publicKeyword, _overrideKeyword]
    );
  private static readonly ExpressionStatementSyntax _provideCall =
    MethodModifier.ThisMemberCallStatement(Constants.PROVIDE_METHOD_NAME, []);
  private static readonly ImmutableArray<string> _fixableDiagnosticIds =
    [Diagnostics.MissingAutoInjectProvideDescriptor.Id];

  public sealed override ImmutableArray<string> FixableDiagnosticIds =>
    _fixableDiagnosticIds;

  public sealed override FixAllProvider GetFixAllProvider() =>
    WellKnownFixAllProviders.BatchFixer;

  public sealed override async Task RegisterCodeFixesAsync(
      CodeFixContext context)
  {
    var root = await context.Document
      .GetSyntaxRootAsync(context.CancellationToken)
      .ConfigureAwait(false);
    if (root is null)
    {
      return;
    }

    var diagnostic = context.Diagnostics.First();
    var diagnosticSpan = diagnostic.Location.SourceSpan;

    // Find the type declaration identified by the diagnostic.
    var typeDeclaration = root
      .FindToken(diagnosticSpan.Start)
      .Parent?
      .AncestorsAndSelf()
      .OfType<TypeDeclarationSyntax>()
      .FirstOrDefault();
    if (typeDeclaration is null)
    {
      return;
    }

    // Register code fixes for either creating or modifying methods that
    // call `this.Provide()`.

    // Setup() Method Fixes
    RegisterMethodFixesAsync(
      context, typeDeclaration, diagnostic,
      Constants.SETUP_METHOD_NAME,
      _setupMethodDeclaration,
      m =>
        m.Identifier.Text == Constants.SETUP_METHOD_NAME
          && m.Modifiers.Any(SyntaxKind.PublicKeyword)
    );

    // OnReady() Method Fixes
    RegisterMethodFixesAsync(
      context, typeDeclaration, diagnostic,
      Constants.ONREADY_METHOD_NAME,
      _onReadyMethodDeclaration,
      m =>
        m.Identifier.Text == Constants.ONREADY_METHOD_NAME
          && m.Modifiers.Any(SyntaxKind.PublicKeyword)
    );

    // _Ready() Method Fixes
    RegisterMethodFixesAsync(
      context, typeDeclaration, diagnostic,
      Constants.READY_METHOD_NAME,
      _readyMethodDeclaration,
      m =>
        m.Identifier.Text == Constants.READY_METHOD_NAME
          && m.Modifiers.Any(SyntaxKind.PublicKeyword)
          && m.Modifiers.Any(SyntaxKind.OverrideKeyword)
    );
  }

  public static string GetCodeFixEquivalenceKey(
      string methodName,
      bool methodExists)
  {
    var operation = methodExists ? "CreateNew" : "AddCallTo";
    return $"{nameof(AutoInjectProvideFixProvider)}_{operation}_{methodName}";
  }

  /// <summary>
  ///   Registers code fixes for a method that either adds a call to an existing
  ///   method if it exists or creates a new method with the call.
  /// </summary>
  /// <param name="context">Code fix context</param>
  /// <param name="typeDeclaration">Type declaration of parent class</param>
  /// <param name="diagnostic">Diagnostic that triggered this</param>
  /// <param name="methodName">Method name to create</param>
  /// <param name="findPredicate">Predicate to find existing method</param>
  /// <param name="creationModifiers">
  ///   Method modifiers to add to created method
  /// </param>
  private static void RegisterMethodFixesAsync(
    CodeFixContext context,
    TypeDeclarationSyntax typeDeclaration,
    Diagnostic diagnostic,
    string methodName,
    MethodDeclarationSyntax newMethod,
    Func<MethodDeclarationSyntax, bool> findPredicate
  )
  {
    var existingMethod = typeDeclaration.Members
      .OfType<MethodDeclarationSyntax>()
      .FirstOrDefault(findPredicate);

    if (existingMethod is not null)
    {
      // Method exists, offer to add a call to it
      context.RegisterCodeFix(
        CodeAction.Create(
          title:
            $"Add \"this.Provide();\" to existing \"{methodName}()\" method",
          createChangedDocument: c =>
            MethodModifier.AddStatementToMethodBodyAsync(
              context.Document,
              existingMethod,
              _provideCall,
              c
            ),
          equivalenceKey: GetCodeFixEquivalenceKey(methodName, true)
        ),
        diagnostic
      );
    }
    else
    {
      // Method does not exist, offer to create it
      context.RegisterCodeFix(
        CodeAction.Create(
          title:
            $"Create \"{methodName}()\" method that calls \"this.Provide()\"",
          createChangedDocument: c =>
            AddNewMethodAsync(
              context.Document,
              typeDeclaration,
              newMethod,
              c
            ),
          equivalenceKey: GetCodeFixEquivalenceKey(methodName, false)
        ),
        diagnostic
      );
    }
  }

  private static async Task<Document> AddNewMethodAsync(
    Document document,
    TypeDeclarationSyntax typeDeclaration,
    MethodDeclarationSyntax newMethod,
    CancellationToken cancellationToken
  )
  {
    var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
    editor.AddMember(typeDeclaration, newMethod);
    return editor.GetChangedDocument();
  }

  private static MethodDeclarationSyntax NewMethod(
    string identifier,
    IEnumerable<SyntaxToken> creationModifiers
  ) =>
    SyntaxFactory
      .MethodDeclaration(
        SyntaxFactory.PredefinedType(
          SyntaxFactory.Token(SyntaxKind.VoidKeyword)
        ),
        identifier
      )
      .WithModifiers(SyntaxFactory.TokenList(creationModifiers))
      .WithBody(
        SyntaxFactory.Block(
          SyntaxFactory.SingletonList(
            SyntaxFactory.ParseStatement(
              Constants.PROVIDE_NEW_METHOD_BODY)
          )
        )
      );
}
