using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VContainer.SourceGenerator
{
    class SyntaxCollector : ISyntaxReceiver
    {
        public List<string> Log { get; } = new();
        public List<WorkItem> WorkItems { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (IsCandidateType(syntaxNode))
            {
                WorkItems.Add(new WorkItem((TypeDeclarationSyntax)syntaxNode));
            }
        }

        static bool IsCandidateType(SyntaxNode syntax)
        {
            if (syntax is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                return false;
            }

            if (classDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.AbstractKeyword) ||
                                                                 modifier.IsKind(SyntaxKind.StaticKeyword)))
            {
                return false;
            }
            return true;
        }
    }
}