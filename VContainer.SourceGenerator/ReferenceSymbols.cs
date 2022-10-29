using Microsoft.CodeAnalysis;

namespace VContainer.SourceGenerator
{
    public class ReferenceSymbols
    {
        public INamedTypeSymbol? VContainerInjectAttribute { get; }
        public INamedTypeSymbol? UnityEngineComponent { get; }

        public ReferenceSymbols(Compilation compilation)
        {
            VContainerInjectAttribute = compilation.GetTypeByMetadataName("VContainer.InjectAttribute");
            UnityEngineComponent = compilation.GetTypeByMetadataName("UnityEngine.Component");
        }
    }
}
