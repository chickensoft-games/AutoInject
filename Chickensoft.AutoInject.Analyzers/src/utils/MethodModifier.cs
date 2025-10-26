namespace Chickensoft.AutoInject.Analyzers.Utils;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

public static class MethodModifier
{
  /// <summary>
  /// Creates a this.method call expression syntax node for the given method
  /// name.
  /// </summary>
  /// <param name="methodToCallName">
  /// The name of the member method to call.
  /// </param>
  /// <returns>
  /// An expression node representing a "this.method()" expression.
  /// </returns>
  public static InvocationExpressionSyntax ThisMemberCallExpression(
    string methodToCallName,
    IEnumerable<string> argumentNames
  )
  {
    var invocation = SyntaxFactory.InvocationExpression(
        SyntaxFactory.MemberAccessExpression(
          SyntaxKind.SimpleMemberAccessExpression,
          SyntaxFactory.ThisExpression(),
          SyntaxFactory.IdentifierName(methodToCallName)
        )
    );
    if (argumentNames.Any())
    {
      invocation = invocation.WithArgumentList(
        SyntaxFactory.ArgumentList(
          SyntaxFactory.SeparatedList(
            argumentNames.Select(
              name =>
                SyntaxFactory.Argument(SyntaxFactory.IdentifierName(name))
            )
          )
        )
      );
    }
    return invocation;
  }

  /// <summary>
  /// Creates a this.method call statement syntax node for the given method
  /// name.
  /// </summary>
  /// <param name="methodToCallName">
  /// The name of the member method to call.
  /// </param>
  /// <returns>
  /// An expression statement node representing a "this.method()" statement.
  /// </returns>
  public static ExpressionStatementSyntax ThisMemberCallStatement(
    string methodToCallName,
    IEnumerable<string> argumentNames
  ) =>
    SyntaxFactory.ExpressionStatement(
      ThisMemberCallExpression(methodToCallName, argumentNames)
    );

  /// <summary>
  /// Adds a provided statement to the end of a specified method's body within a
  /// type declaration. Handles conversion from expression body to block body.
  /// </summary>
  /// <param name="document">Document to modify.</param>
  /// <param name="typeDeclaration">Type declaration of the class.</param>
  /// <param name="originalMethodNode">
  /// The method to add the statement to.
  /// </param>
  /// <param name="statementToAdd">The pre-constructed statement to add.</param>
  /// <param name="cancellationToken">Cancellation Token.</param>
  /// <returns>Modified document.</returns>
  public static async Task<Document> AddStatementToMethodBodyAsync(
    Document document,
    MethodDeclarationSyntax originalMethodNode,
    StatementSyntax statementToAdd,
    CancellationToken cancellationToken
  )
  {
    var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

    List<SyntaxNode> statements = [];
    if (originalMethodNode.Body is not null)
    {
      statements.AddRange(editor.Generator.GetStatements(originalMethodNode));
    }
    else
    {
      var exprBodyExpr = editor.Generator.GetExpression(originalMethodNode) as ExpressionSyntax;
      if (exprBodyExpr is not null)
      {
        var statementFromExpr = SyntaxFactory.ExpressionStatement(exprBodyExpr);
        // Make sure to preserve the trailing trivia from the original method's semicolon token
        // If we don't do this we will lose any code comments or whitespace that was after the semicolon
        var originalMethodSemicolon = originalMethodNode.SemicolonToken;
        if (
          !originalMethodSemicolon.IsKind(SyntaxKind.None)
            && !originalMethodSemicolon.IsMissing
        )
        {
          var originalSemicolonTrailingTrivia = originalMethodSemicolon
            .TrailingTrivia;
          if (originalSemicolonTrailingTrivia.Any())
          {
            statementFromExpr = statementFromExpr
              .WithSemicolonToken(
                statementFromExpr
                  .SemicolonToken
                  .WithTrailingTrivia(originalSemicolonTrailingTrivia)
              );
          }
        }
        statements.Add(statementFromExpr);
      }
    }
    statements.Add(statementToAdd);

    editor.SetStatements(originalMethodNode, statements);

    return editor.GetChangedDocument();
  }
}
