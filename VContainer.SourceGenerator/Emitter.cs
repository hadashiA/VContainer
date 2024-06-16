using System.Linq;
using Microsoft.CodeAnalysis;
#if ROSLYN3
using SourceProductionContext = Microsoft.CodeAnalysis.GeneratorExecutionContext;
#endif

namespace VContainer.SourceGenerator;

static class Emitter
{
    public static bool TryEmitGeneratedInjector(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        ReferenceSymbols references,
        in SourceProductionContext context)
    {
        if (typeMeta.IsNested())
        {
            if (typeMeta.ExplicitInjectable)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.NestedNotSupported,
                    typeMeta.GetLocation(),
                    typeMeta.Symbol.Name));
            }
            return false;
        }

        if (typeMeta.Symbol.IsAbstract)
        {
            if (typeMeta.ExplicitInjectable)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.AbstractNotAllow,
                    typeMeta.GetLocation(),
                    typeMeta.TypeName));
            }
            return false;
        }

        if (typeMeta.IsGenerics)
        {
            return false; // TODO:
        }

        codeWriter.AppendLine("using System;");
        codeWriter.AppendLine("using System.Collections.Generic;");
        codeWriter.AppendLine("using VContainer;");
        codeWriter.AppendLine();

        var ns = typeMeta.Symbol.ContainingNamespace;
        if (!ns.IsGlobalNamespace)
        {
            codeWriter.AppendLine($"namespace {ns}");
            codeWriter.BeginBlock();
        }

        var typeName = typeMeta.TypeName
            .Replace("global::", "")
            .Replace("<", "_")
            .Replace(">", "_");

        var generateTypeName = $"{typeName}GeneratedInjector";

        codeWriter.AppendLine("[Preserve]");
        using (codeWriter.BeginBlockScope($"class {generateTypeName} : IInjector"))
        {
            codeWriter.AppendLine();
            if (!TryEmitCreateInstanceMethod(typeMeta, codeWriter, references, in context))
            {
                return false;
            }

            codeWriter.AppendLine();

            if (!TryEmitInjectMethod(typeMeta, codeWriter, in context))
            {
                return false;
            }
        }

        if (!ns.IsGlobalNamespace)
        {
            codeWriter.EndBlock();
        }

        return true;
    }

    static bool TryEmitInjectMethod(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        in SourceProductionContext context)
    {
        using (codeWriter.BeginBlockScope(
                   "public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)"))
        {
            if (typeMeta.InjectFields.Count <= 0 &&
                typeMeta.InjectProperties.Count <= 0 &&
                typeMeta.InjectMethods.Count <= 0)
            {
                codeWriter.AppendLine("return;");
                return true;
            }

            codeWriter.AppendLine($"var __x = ({typeMeta.TypeName})instance;");

            var error = false;

            // verify field
            foreach (var fieldSymbol in typeMeta.InjectFields)
            {
                if (!fieldSymbol.CanBeCallFromInternal())
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.PrivateFieldNotSupported,
                        fieldSymbol.Locations.FirstOrDefault() ?? typeMeta.GetLocation(),
                        fieldSymbol.Name));
                    error = true;
                }

                if (fieldSymbol.Type is ITypeParameterSymbol)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.GenericsNotSupported,
                        fieldSymbol.Locations.FirstOrDefault() ?? typeMeta.GetLocation(),
                        fieldSymbol.Name));
                    error = true;
                }
            }

            // verify property
            foreach (var propSymbol in typeMeta.InjectProperties)
            {
                if (!propSymbol.CanBeCallFromInternal())
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.PrivatePropertyNotSupported,
                        propSymbol.Locations.FirstOrDefault() ?? typeMeta.GetLocation(),
                        propSymbol.Name));
                    error = true;
                }

                if (propSymbol.Type is ITypeParameterSymbol)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.GenericsNotSupported,
                        propSymbol.Locations.FirstOrDefault() ?? typeMeta.GetLocation(),
                        propSymbol.Name));
                    error = true;
                }
            }

            // verify method
            if (typeMeta.InjectMethods.Count > 1)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.GenericsNotSupported,
                    typeMeta.InjectMethods.First().Locations.FirstOrDefault() ?? typeMeta.GetLocation(),
                    typeMeta.InjectMethods.First().Name));
                error = true;
            }

            foreach (var methodSymbol in typeMeta.InjectMethods)
            {
                if (!methodSymbol.CanBeCallFromInternal())
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.PrivateMethodNotSupported,
                        methodSymbol.Locations.FirstOrDefault() ?? typeMeta.GetLocation(),
                        methodSymbol.Name));
                    error = true;
                }
                if (methodSymbol.Arity > 0)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.GenericsNotSupported,
                        methodSymbol.Locations.FirstOrDefault() ?? typeMeta.GetLocation(),
                        methodSymbol.Name));
                    error = true;
                }
            }

            if (error)
            {
                return false;
            }

            foreach (var fieldSymbol in typeMeta.InjectFields)
            {
                var fieldTypeName = fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                codeWriter.AppendLine($"__x.{fieldSymbol.Name} = ({fieldTypeName})resolver.ResolveOrParameter(typeof({fieldTypeName}), \"{fieldSymbol.Name}\", parameters);");
            }

            foreach (var propSymbol in typeMeta.InjectProperties)
            {
                var propTypeName = propSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                codeWriter.AppendLine($"__x.{propSymbol.Name} = ({propTypeName})resolver.ResolveOrParameter(typeof({propTypeName}), \"{propSymbol.Name}\", parameters);");
            }

            foreach (var methodSymbol in typeMeta.InjectMethods)
            {
                var parameters = methodSymbol.Parameters
                    .Select(param =>
                    {
                        var paramType =
                            param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        var paramName = param.Name;
                        return (paramType, paramName);
                    })
                    .ToArray();

                foreach (var (paramType, paramName) in parameters)
                {
                    codeWriter.AppendLine(
                        $"var __{paramName} = resolver.ResolveOrParameter(typeof({paramType}), \"{paramName}\", parameters);");
                }

                var arguments = parameters.Select(x => $"({x.paramType})__{x.paramName}");
                codeWriter.AppendLine($"__x.{methodSymbol.Name}({string.Join(", ", arguments)});");
            }
            return true;
        }
    }

    public static bool TryEmitCreateInstanceMethod(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        ReferenceSymbols references,
        in SourceProductionContext context)
    {
        if (typeMeta.ExplictInjectConstructors.Count > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MultipleCtorAttributeNotSupported,
                typeMeta.GetLocation(),
                typeMeta.TypeName));
            return false;
        }

        var constructorSymbol = typeMeta.ExplictInjectConstructors.Count == 1
            ? typeMeta.ExplictInjectConstructors.First()
            : typeMeta.Constructors.OrderByDescending(ctor => ctor.Parameters.Length).FirstOrDefault();

        if (constructorSymbol != null)
        {
            if (!constructorSymbol.CanBeCallFromInternal())
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.PrivateConstructorNotSupported,
                    typeMeta.GetLocation(),
                    typeMeta.TypeName));
                return false;
            }

            if (constructorSymbol.Arity > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.GenericsNotSupported,
                    typeMeta.GetLocation(),
                    typeMeta.TypeName));
                return false;
            }
        }

        using (codeWriter.BeginBlockScope("public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)"))
        {
            if (references.UnityEngineComponent != null &&
                typeMeta.InheritsFrom(references.UnityEngineComponent))
            {
                codeWriter.AppendLine($"throw new NotSupportedException(\"UnityEngine.Component:{typeMeta.TypeName} cannot be `new`\");");
                return true;
            }
            if (constructorSymbol is null)
            {
                codeWriter.AppendLine($"var __instance = new {typeMeta.TypeName}();");
                codeWriter.AppendLine("Inject(__instance, resolver, parameters);");
                codeWriter.AppendLine("return __instance;");
                return true;
            }
            var parameters = constructorSymbol.Parameters
                .Select(param =>
                {
                    var paramType =
                        param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var paramName = param.Name;
                    return (paramType, paramName);
                })
                .ToArray();

            foreach (var (paramType, paramName) in parameters)
            {
                codeWriter.AppendLine(
                    $"var __{paramName} = resolver.ResolveOrParameter(typeof({paramType}), \"{paramName}\", parameters);");
            }

            var arguments = parameters.Select(x => $"({x.paramType})__{x.paramName}");
            codeWriter.AppendLine($"var __instance = new {typeMeta.TypeName}({string.Join(", ", arguments)});");
            codeWriter.AppendLine("Inject(__instance, resolver, parameters);");
            codeWriter.AppendLine("return __instance;");
        }
        return true;
    }
}
