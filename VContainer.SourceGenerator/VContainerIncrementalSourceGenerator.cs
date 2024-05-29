using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VContainer.SourceGenerator;

[Generator]
public class VContainerIncrementalSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Assembly filter
        var vcontainerReferenceValueProvider = context.CompilationProvider
            .Select((compilation, cancellation) =>
            {
                if (compilation.AssemblyName?.StartsWith("VContainer") == true &&
                    !compilation.AssemblyName.Contains("Test"))
                {
                    return false;
                }

                if (compilation.AssemblyName?.StartsWith("UnityEngine.") == true ||
                    compilation.AssemblyName?.StartsWith("Unity.") == true)
                {
                    return false;
                }

                foreach (var referencedAssemblyName in compilation.ReferencedAssemblyNames)
                {
                    if (referencedAssemblyName.Name.StartsWith("VContainer"))
                        return true;
                }
                return false;
            });

        // Find Types based on Register* methods
        var registerInvocations = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, cancellation) =>
                {
                    if (s.IsKind(SyntaxKind.InvocationExpression))
                    {
                        if (s is InvocationExpressionSyntax
                            {
                                Expression: MemberAccessExpressionSyntax
                                {
                                    Expression: IdentifierNameSyntax
                                } memberAccess
                            })
                        {
                            if (memberAccess.Name.Identifier.Text.StartsWith("Register"))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                },
                (ctx, cancellation) => ctx)
            .Combine(vcontainerReferenceValueProvider)
            .Where(tuple => tuple.Right)
            .Select((tuple, cancellation) =>
            {
                var invocationExpressionSyntax = (InvocationExpressionSyntax)tuple.Left.Node;
                var semanticModel = tuple.Left.SemanticModel;
                return new RegisterInvocationCandidate(invocationExpressionSyntax, semanticModel);
            });

        // Find types by explicit [Inject]
        var typeDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider((s, cancellation) =>
                {
                    if (!s.IsKind(SyntaxKind.ClassDeclaration)) return false;
                    if (s is not ClassDeclarationSyntax syntax) return false;

                    if (syntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.AbstractKeyword) ||
                                                         modifier.IsKind(SyntaxKind.StaticKeyword)))
                    {
                        return false;
                    }

                    return true;
                },
                (ctx, cancellation) => ctx)
            .Combine(vcontainerReferenceValueProvider)
            .Where(tuple => tuple.Right)
            .Select((tuple, cancellatio) =>
            {
                return new TypeDeclarationCandidate((TypeDeclarationSyntax)tuple.Left.Node, tuple.Left.SemanticModel);
            });

        // Generate the source code.
        context.RegisterSourceOutput(
            context.CompilationProvider
                .Combine(typeDeclarations.Collect())
                .Combine(registerInvocations.Collect()),
            (sourceProductionContext, tuple) =>
            {
                var compilation = tuple.Left.Left;
                var typeDeclarationCandidates = tuple.Left.Right;
                var registerInvocationCandidates = tuple.Right;

                var references = ReferenceSymbols.Create(compilation);
                if (references is null)
                {
                    return;
                }

                var codeWriter = new CodeWriter();

                var typeMetas = typeDeclarationCandidates
                    .Select(x => x.Analyze(references))
                    .Where(x => x != null &&
                                (x.ExplictInjectConstructors.Count > 0 ||
                                 x.InjectFields.Count > 0 ||
                                 x.InjectProperties.Count > 0 ||
                                 x.InjectMethods.Count > 0));

                var typeMetasFromRegister = registerInvocationCandidates
                    .SelectMany(x => x.Analyze(references));

                foreach (var typeMeta in typeMetas
                             .Concat(typeMetasFromRegister)
                             .Where(x => x != null)
                             .DistinctBy(x => x!.Symbol, SymbolEqualityComparer.Default))
                {
                    if (Emitter.TryEmitGeneratedInjector(typeMeta!, codeWriter, references, in sourceProductionContext))
                    {
                        var fullType = typeMeta!.FullTypeName
                            .Replace("global::", "")
                            .Replace("<", "_")
                            .Replace(">", "_");
                        sourceProductionContext.AddSource($"{fullType}GeneratedInjector.g.cs", codeWriter.ToString());
                    }
                    codeWriter.Clear();
                }
            });
    }
}
