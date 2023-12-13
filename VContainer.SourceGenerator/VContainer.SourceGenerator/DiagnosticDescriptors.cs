using Microsoft.CodeAnalysis;

namespace VContainer.SourceGenerator
{
    static class DiagnosticDescriptors
    {
        const string Category = "VContainer.SourceGenerator.Roslyn3";

        public static readonly DiagnosticDescriptor UnexpectedErrorDescriptor = new(
            id: "VCON0001",
            title: "Unexpected error during generation",
            messageFormat: "Unexpected error occurred during code generation: {0}",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor AbstractNotAllow = new(
            id: "VCON0002",
            title: "Injectable type must not be abstract/interface",
            messageFormat: "The injectable type of '{0}' is abstract/interface. It is not allowed",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MultipleCtorAttributeNotSupported = new(
            id: "VCON0003",
            title: "[Inject] exists in multiple constructors",
            messageFormat: "Multiple [Inject] constructors exists in '{0}'",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MultipleInjectMethodNotSupported = new(
            id: "VCON0004",
            title: "[Inject] exists in multiple methods",
            messageFormat: "Multiple [Inject] methods exists in '{0}'",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NestedNotSupported = new(
            id: "VCON0005",
            title: "Nested type is not support to code generation.",
            messageFormat: "The injectable object '{0}' is a nested type. It cannot support code generation ",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrivateConstructorNotSupported = new(
            id: "VCON0006",
            title: "The private constructor is not supported to code generation.",
            messageFormat: "The injectable constructor of '{0}' is private. It cannot support source generator.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrivateFieldNotSupported = new(
            id: "VCON0007",
            title: "The private [Inject] field is not supported to code generation.",
            messageFormat: "The [Inject] field '{0}' does not have accessible to set from the same dll. It cannot support to inject by the source generator.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrivatePropertyNotSupported = new(
            id: "VCON0008",
            title: "The private [Inject] property is not supported to code generation",
            messageFormat: "The [Inject] '{0}' does not have accessible to set from the same dll. It cannot support to inject by the source generator.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor PrivateMethodNotSupported = new(
            id: "VCON0009",
            title: "The private [Inject] method is not supported to code generation.",
            messageFormat: "The [Inject] '{0}' does not have accessible to call from the same dll. It cannot support inject by the source generator.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor GenericsNotSupported = new(
            id: "VCON0010",
            title: "The [Inject] constructor or method that require generics argument is not supported to code generation.",
            messageFormat: "[Inject] '{0}' needs generic arguments. It cannot inject by the source generator.",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
