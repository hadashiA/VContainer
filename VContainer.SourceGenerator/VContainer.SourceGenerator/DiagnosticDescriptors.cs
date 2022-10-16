using Microsoft.CodeAnalysis;

namespace VContainer.SourceGenerator
{
    static class DiagnosticDescriptors
    {
        const string Category = "VContainer.SourceGenerator";

        public static readonly DiagnosticDescriptor UnexpectedErrorDescriptor = new(
            id: "VCON0001",
            title: "Unexpected error during generation",
            messageFormat: "Unexpected error occurred during code generation: {0}",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MultipleCtorWithoutAttribute = new(
            id: "VCON0002",
            title: "Require [Inject] when exists multiple constructors",
            messageFormat: "The inject target '{0}' must annotate with [Inject] when exists multiple constructors",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MultipleCtorAttribute = new(
            id: "VCON0003",
            title: "[Inject] exists in multiple constructors",
            messageFormat: "Mupltiple [Inject] exists in '{0}' but allows only single ctor",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
