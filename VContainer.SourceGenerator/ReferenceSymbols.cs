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
                UnityEngineComponent = compilation.GetTypeByMetadataName("UnityEngine.Component"),
            };
        }

        public INamedTypeSymbol VContainerInjectAttribute { get; private set; }
        public INamedTypeSymbol? UnityEngineComponent { get; private set; }
    }
}
