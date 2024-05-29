using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VContainer.SourceGenerator;

class SyntaxCollector : ISyntaxReceiver
{
    public List<string> Log { get; } = new();
    public List<WorkItem> WorkItems { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode.IsKind(SyntaxKind.ClassDeclaration))
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                if (!classDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.AbstractKeyword) ||
                                                                      modifier.IsKind(SyntaxKind.StaticKeyword)))
                {
                    WorkItems.Add(new WorkItem(classDeclarationSyntax));
                }
            }
        }
        else if (syntaxNode.IsKind(SyntaxKind.InvocationExpression))
        {
            if (syntaxNode is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Expression: IdentifierNameSyntax
                    } memberAccess
                } invocationExpressionSyntax)
            {
                if (memberAccess.Name.Identifier.Text.StartsWith("Register"))
                {
                    WorkItems.Add(new WorkItem(invocationExpressionSyntax));
                }
            }
        }
    }
}
