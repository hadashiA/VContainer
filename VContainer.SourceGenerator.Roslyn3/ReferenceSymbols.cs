using Microsoft.CodeAnalysis;

namespace VContainer.SourceGenerator
{
    public class ReferenceSymbols
    {
        public static ReferenceSymbols? Create(Compilation compilation)
        {
            var injectAttribute = compilation.GetTypeByMetadataName("VContainer.InjectAttribute");
            if (injectAttribute is null)
                return null;

            return new ReferenceSymbols
            {
                VContainerInjectAttribute = injectAttribute,
                VContainerInjectIgnoreAttribute = compilation.GetTypeByMetadataName("VContainer.InjectIgnoreAttribute")!,
                AttributeBase = compilation.GetTypeByMetadataName("System.Attribute")!,
                UnityEngineComponent = compilation.GetTypeByMetadataName("UnityEngine.Component"),
            };
        }

        public INamedTypeSymbol VContainerInjectAttribute { get; private set; } = default!;
        public INamedTypeSymbol VContainerInjectIgnoreAttribute { get; private set; } = default!;
        public INamedTypeSymbol AttributeBase { get; private set; } = default!;
        public INamedTypeSymbol? UnityEngineComponent { get; private set; }
    }
}
