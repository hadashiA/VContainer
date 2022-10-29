using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace VContainer.SourceGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxCollector());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var moduleName = context.Compilation.SourceModule.Name;
                if (moduleName.StartsWith("UnityEngine.")) return;
                if (moduleName.StartsWith("UnityEditor.")) return;
                if (moduleName.StartsWith("Unity.")) return;
                if (moduleName.StartsWith("VContainer.")) return;

                var references = new ReferenceSymbols(context.Compilation);
                if (references.VContainerInjectAttribute is null) return;

                var codeWriter = new CodeWriter();
                var syntaxCollector = (SyntaxCollector)context.SyntaxReceiver!;
                var l = new List<TypeMeta>();
                foreach (var workItem in syntaxCollector.WorkItems)
                {
                    var typeMeta = workItem.Analyze(in context, references);
                    if (typeMeta is null) continue;
                    if (!typeMeta.Symbol.CanBeCallFromInternal()) continue;

                    EmitGeneratedInjector(typeMeta, codeWriter, references, in context);
                    codeWriter.Clear();
                    l.Add(typeMeta);
                }

                using (codeWriter.BeginBlockScope("class Unchi"))
                {
                    foreach (var typeMeta in l)
                    {
                        codeWriter.AppendLine($"// {typeMeta.TypeName}");
                    }
                }
                context.AddSource("Unchi.cs", codeWriter.ToString());
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.UnexpectedErrorDescriptor,
                    Location.None,
                    ex.ToString().Replace(Environment.NewLine, " ")));
            }
        }

        static void EmitGeneratedInjector(
            TypeMeta typeMeta,
            CodeWriter codeWriter,
            ReferenceSymbols references,
            in GeneratorExecutionContext context)
        {
            try
            {
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

                var fullType = typeMeta.FullTypeName
                    .Replace("global::", "")
                    .Replace("<", "_")
                    .Replace(">", "_");

                var generateTypeName = $"{fullType}VContainerGeneratedInjector";
                using (codeWriter.BeginBlockScope($"class {generateTypeName} : IInjector"))
                {
                    if (!TryEmitInjectMethod(typeMeta, codeWriter, in context))
                    {
                        return;
                    }

                    codeWriter.AppendLine();

                    if (!TryEmitCreateInstanceMethod(typeMeta, codeWriter, references, in context))
                    {
                        return;
                    }
                }

                if (!ns.IsGlobalNamespace)
                {
                    codeWriter.EndBlock();
                }

                context.AddSource($"{generateTypeName}.cs", codeWriter.ToString());
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.UnexpectedErrorDescriptor,
                    Location.None,
                    ex.ToString().Replace(Environment.NewLine, " ")));
            }
        }

        static bool TryEmitInjectMethod(
            TypeMeta typeMeta,
            CodeWriter codeWriter,
            in GeneratorExecutionContext context)
        {
            using (codeWriter.BeginBlockScope(
                       "public void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)"))
            {
                var injectFields = typeMeta.GetInjectFields();
                var injectProperties = typeMeta.GetInjectProperties();
                var injectMethods = typeMeta.GetInjectMethods();

                if (injectFields.Count == 0 &&
                    injectProperties.Count == 0 &&
                    injectMethods.Count == 0)
                {
                    codeWriter.AppendLine("return;");
                    return true;
                }

                codeWriter.AppendLine($"var x = ({typeMeta.TypeName})instance;");

                foreach (var fieldSymbol in injectFields)
                {
                    if (!fieldSymbol.CanBeCallFromInternal())
                    {
                        var invalid = Diagnostic.Create(
                            DiagnosticDescriptors.CannotAccessInjectField,
                            fieldSymbol.Locations.FirstOrDefault() ?? typeMeta.Syntax.GetLocation(),
                            fieldSymbol.Name);
                        context.ReportDiagnostic(invalid);
                        return false;
                    }

                    var fieldTypeName =
                        fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                    codeWriter.AppendLine($"x.{fieldSymbol.Name} = resolver.Resolve<{fieldTypeName}>();");
                }

                foreach (var propSymbol in injectProperties)
                {
                    if (!propSymbol.CanBeCallFromInternal())
                    {
                        var invalid = Diagnostic.Create(
                            DiagnosticDescriptors.CannotAccessInjectProperty,
                            propSymbol.Locations.FirstOrDefault() ?? typeMeta.Syntax.GetLocation(),
                            propSymbol.Name);
                        context.ReportDiagnostic(invalid);
                        return false;
                    }

                    var propTypeName =
                        propSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    codeWriter.AppendLine($"x.{propSymbol.Name} = resolver.Resolve<{propTypeName}>();");
                }

                foreach (var methodSymbol in injectMethods)
                {
                    if (!methodSymbol.CanBeCallFromInternal())
                    {
                        var invalid = Diagnostic.Create(
                            DiagnosticDescriptors.CannotAccessInjectMethod,
                            methodSymbol.Locations.FirstOrDefault() ?? typeMeta.Syntax.GetLocation(),
                            methodSymbol.Name);
                        context.ReportDiagnostic(invalid);
                        return false;
                    }

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
                            $"var {paramName} = resolver.ResolveOrParameter(typeof({paramType}), \"{paramName}\", parameters);");
                    }

                    var arguments = parameters.Select(x => $"{x.paramType} {x.paramName}");
                    codeWriter.AppendLine($"x.{methodSymbol.Name}({string.Join(", ", arguments)});");
                }
                return true;
            }
        }

        static bool TryEmitCreateInstanceMethod(
            TypeMeta typeMeta,
            CodeWriter codeWriter,
            ReferenceSymbols references,
            in GeneratorExecutionContext context)
        {
            var constructorSymbol = typeMeta.GetInjectConstructor(out var invalid);
            if (invalid != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(invalid, typeMeta.Syntax.GetLocation(), typeMeta.Symbol.Name));
                return false;
            }

            using (codeWriter.BeginBlockScope("public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)"))
            {
                if (typeMeta.InheritsFrom(references.UnityEngineComponent))
                {
                    codeWriter.AppendLine($"throw new NotSupportedException(\"UnityEngine.Component:{typeMeta.TypeName} cannot be `new`\");");
                    return true;
                }
                if (constructorSymbol is null)
                {
                    codeWriter.AppendLine($"return new {typeMeta.TypeName}();");
                    return true;
                }
                var parameters = constructorSymbol.Parameters
                    .Select(param =>
                    {
                        var paramType =
                            param.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                        var paramName = param.Name;
                        return (paramType, paramName);
                    })
                    .ToArray();

                foreach (var (paramType, paramName) in parameters)
                {
                    codeWriter.AppendLine(
                        $"var {paramName} = resolver.ResolveOrParameter(typeof({paramType}), \"{paramName}\", parameters);");
                }

                var arguments = parameters.Select(x => $"{x.paramType} {x.paramName}");
                codeWriter.AppendLine($"return new {typeMeta.FullTypeName}({string.Join(", ", arguments)});");
            }
            return true;
        }
    }
}
