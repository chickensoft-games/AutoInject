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
using Microsoft.CodeAnalysis.Editing;
using Utils;

[
  ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(AutoInjectNotificationOverrideFixProvider)
  ),
  Shared
]
public class AutoInjectNotificationOverrideFixProvider : CodeFixProvider
{
  private static readonly MethodDeclarationSyntax _notificationDeclaration =
    SyntaxFactory.MethodDeclaration(
      SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
      Constants.NOTIFICATION_METHOD_NAME
    )
    .WithModifiers(
      SyntaxFactory.TokenList(
        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
        SyntaxFactory.Token(SyntaxKind.OverrideKeyword)
      )
    )
    .WithParameterList(
      SyntaxFactory.ParameterList(
        SyntaxFactory.SingletonSeparatedList(
          SyntaxFactory
            .Parameter(SyntaxFactory.Identifier(Constants.WHAT_PARAMETER_NAME))
            .WithType(
              SyntaxFactory.PredefinedType(
                SyntaxFactory.Token(SyntaxKind.IntKeyword)
              )
            )
        )
      )
    )
    .WithExpressionBody(
      SyntaxFactory
        .ArrowExpressionClause(
          MethodModifier.ThisMemberCallExpression(
            Constants.NOTIFY_METHOD_NAME,
            [Constants.WHAT_PARAMETER_NAME]
          )
        )
    )
    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

  private static readonly ImmutableArray<string> _fixableDiagnosticIds =
    [Diagnostics.MissingAutoInjectNotificationOverrideDescriptor.Id];

  public sealed override ImmutableArray<string> FixableDiagnosticIds =>
    _fixableDiagnosticIds;

  public sealed override FixAllProvider GetFixAllProvider() =>
    WellKnownFixAllProviders.BatchFixer;

  public sealed override async Task RegisterCodeFixesAsync(
    CodeFixContext context
  )
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

    // Find the type declaration identified by the diagnostic
    var typeDeclaration = root
      .FindNode(diagnosticSpan)
      .AncestorsAndSelf()
      .OfType<TypeDeclarationSyntax>()
      .FirstOrDefault();
    if (typeDeclaration is null)
    {
      return;
    }

    context.RegisterCodeFix(
      // new AutoInjectNotificationOverrideCodeAction(context.Document, diagnostic),
      CodeAction.Create(
        "Add \"public override void _Notification(int what) => this.Notify(what);\" method",
        cancellationToken =>
          AddAutoInjectNotificationOverrideAsync(
            context.Document,
            typeDeclaration,
            cancellationToken
          ),
        nameof(AutoInjectNotificationOverrideFixProvider)
      ),
      diagnostic
    );
  }

  private static async Task<Document> AddAutoInjectNotificationOverrideAsync(
    Document document,
    TypeDeclarationSyntax typeDeclaration,
    CancellationToken cancellationToken
  )
  {
    var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
    editor.InsertMembers(typeDeclaration, 0, [_notificationDeclaration]);
    return editor.GetChangedDocument();
  }
}
