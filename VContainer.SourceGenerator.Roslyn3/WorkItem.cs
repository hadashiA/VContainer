using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VContainer.SourceGenerator;

class WorkItem
{
    public TypeDeclarationSyntax? TypeDeclarationSyntax { get; }
    public InvocationExpressionSyntax? RegisterInvocationSyntax { get; }

    public WorkItem(TypeDeclarationSyntax typeDeclarationSyntax)
    {
        TypeDeclarationSyntax = typeDeclarationSyntax;
    }

    public WorkItem(InvocationExpressionSyntax registerInvocationCandidate)
    {
        RegisterInvocationSyntax = registerInvocationCandidate;
    }
}