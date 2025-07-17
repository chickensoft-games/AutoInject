namespace Chickensoft.AutoInject.Analyzers.Utils;

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class AnalyzerTools {
  public static AttributeSyntax? GetAutoInjectMetaAttribute(
    ClassDeclarationSyntax classDeclaration,
    Func<string, bool> isMetaName
  ) =>
    classDeclaration.
      AttributeLists
      .SelectMany(list => list.Attributes)
      .FirstOrDefault(
        attr =>
          attr.ArgumentList is not null
            && attr.Name.ToString() == Constants.META_ATTRIBUTE_NAME
            && attr.ArgumentList.Arguments.Any(
              arg =>
                arg.Expression is TypeOfExpressionSyntax {
                  Type: IdentifierNameSyntax identifierName
                }
                  && isMetaName(identifierName.Identifier.ValueText)
            )
      );

  public static MethodDeclarationSyntax? GetMethodOverride(
    TypeDeclarationSyntax typeDeclaration,
    string methodName
  ) =>
    typeDeclaration
      .Members
      .OfType<MethodDeclarationSyntax>()
      .FirstOrDefault(
        method =>
          method.ParameterList.Parameters.Count == 1
            && method.Identifier.ValueText == methodName
            && method.Modifiers.Any(SyntaxKind.OverrideKeyword)
      );

  public static bool HasThisCall(
    MemberDeclarationSyntax node,
    string methodName
  ) =>
    node
      .DescendantNodes()
      .OfType<InvocationExpressionSyntax>()
      .Any(
        invocation =>
          invocation.Expression is MemberAccessExpressionSyntax {
            Expression: ThisExpressionSyntax
          } memberInvocation
            && memberInvocation.Name.Identifier.ValueText == methodName
      );
}
