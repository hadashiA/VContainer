using System.Linq;
using Microsoft.CodeAnalysis;
#if ROSLYN3
using SourceProductionContext = Microsoft.CodeAnalysis.GeneratorExecutionContext;
#endif
using System.Text;
using System.Collections.Generic;

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
                if (propSymbol.SetMethod == null ||
                    propSymbol.SetMethod.IsInitOnly ||
                    !propSymbol.SetMethod.CanBeCallFromInternal())
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
                EmitFieldInjection(codeWriter, fieldSymbol);
            }

            foreach (var propSymbol in typeMeta.InjectProperties)
            {
                EmitPropertyInjection(codeWriter, propSymbol);
            }

            foreach (var methodSymbol in typeMeta.InjectMethods)
            {
                EmitParameterizedMethodCall(codeWriter, methodSymbol);
            }
            return true;
        }
    }

    static void EmitFieldInjection(CodeWriter codeWriter, IFieldSymbol field)
    {
        var fieldType = field.Type;
        var fieldName = field.Name;
        
        string id = null;
        foreach (var attribute in field.GetAttributes())
        {
            if (attribute.AttributeClass.Name == "InjectWithIdAttribute" && attribute.ConstructorArguments.Length > 0)
            {
                id = attribute.ConstructorArguments[0].Value.ToString();
                break;
            }
        }

        if (string.IsNullOrEmpty(id))
        {
            codeWriter.AppendLine($"__x.{fieldName} = ({EmitParamType(fieldType)})resolver.ResolveOrParameter(typeof({EmitParamType(fieldType)}), \"{fieldName}\", parameters);");
        }
        else
        {
            codeWriter.AppendLine($"__x.{fieldName} = ({EmitParamType(fieldType)})resolver.ResolveOrParameter(typeof({EmitParamType(fieldType)}), \"{fieldName}\", \"{id}\", parameters);");
        }
    }

    static void EmitPropertyInjection(CodeWriter codeWriter, IPropertySymbol property)
    {
        var propertyType = property.Type;
        var propertyName = property.Name;
        
        string id = null;
        foreach (var attribute in property.GetAttributes())
        {
            if (attribute.AttributeClass.Name == "InjectWithIdAttribute" && attribute.ConstructorArguments.Length > 0)
            {
                id = attribute.ConstructorArguments[0].Value.ToString();
                break;
            }
        }

        if (string.IsNullOrEmpty(id))
        {
            codeWriter.AppendLine($"__x.{propertyName} = ({EmitParamType(propertyType)})resolver.ResolveOrParameter(typeof({EmitParamType(propertyType)}), \"{propertyName}\", parameters);");
        }
        else
        {
            codeWriter.AppendLine($"__x.{propertyName} = ({EmitParamType(propertyType)})resolver.ResolveOrParameter(typeof({EmitParamType(propertyType)}), \"{propertyName}\", \"{id}\", parameters);");
        }
    }

    static void EmitParameterizedMethodCall(CodeWriter codeWriter, IMethodSymbol methodSymbol)
    {
        var parameters = methodSymbol.Parameters;
        var parameterVariableNames = new List<string>();
        
        var methodName = methodSymbol.Name;
        var methodAccess = methodSymbol.IsStatic ? $"{EmitTypeName(methodSymbol.ContainingType)}" : "__x";

        using (codeWriter.BeginBlockScope())
        {
            foreach (var parameter in parameters)
            {
                var parameterType = parameter.Type;
                var parameterName = parameter.Name;
                var parameterVariableName = "param_" + parameterName;
                parameterVariableNames.Add(parameterVariableName);
                
                string id = null;
                foreach (var attribute in parameter.GetAttributes())
                {
                    if (attribute.AttributeClass.Name == "InjectWithIdAttribute" && attribute.ConstructorArguments.Length > 0)
                    {
                        id = attribute.ConstructorArguments[0].Value.ToString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(id))
                {
                    codeWriter.AppendLine($"var {parameterVariableName} = ({EmitParamType(parameterType)})resolver.ResolveOrParameter(typeof({EmitParamType(parameterType)}), \"{parameterName}\", parameters);");
                }
                else
                {
                    codeWriter.AppendLine($"var {parameterVariableName} = ({EmitParamType(parameterType)})resolver.ResolveOrParameter(typeof({EmitParamType(parameterType)}), \"{parameterName}\", \"{id}\", parameters);");
                }
            }
            if (!methodSymbol.ReturnsVoid)
            {
                codeWriter.AppendLine($"var result = {methodAccess}.{methodName}({string.Join(", ", parameterVariableNames)});");
            }
            else
            {
                codeWriter.AppendLine($"{methodAccess}.{methodName}({string.Join(", ", parameterVariableNames)});");
            }
        }
    }

    static void EmitParameterizedConstructorCall(StringBuilder injectMethodCode, CodeWriter writer, IMethodSymbol constructorSymbol, TypeMeta typeMeta)
    {
        injectMethodCode.AppendLine($"var __instance = new {typeMeta.TypeName}(");
        writer.IncreasaeIndent();
        writer.IncreasaeIndent();

        var parameters = constructorSymbol.Parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var parameterType = parameter.Type;
            var parameterName = parameter.Name;
            
            string id = null;
            foreach (var attribute in parameter.GetAttributes())
            {
                if (attribute.AttributeClass.Name == "InjectWithIdAttribute" && attribute.ConstructorArguments.Length > 0)
                {
                    id = attribute.ConstructorArguments[0].Value.ToString();
                    break;
                }
            }

            if (i + 1 < parameters.Length)
            {
                if (string.IsNullOrEmpty(id))
                {
                    injectMethodCode.AppendLine($"({EmitParamType(parameterType)})resolver.ResolveOrParameter(typeof({EmitParamType(parameterType)}), \"{parameterName}\", parameters),");
                }
                else
                {
                    injectMethodCode.AppendLine($"({EmitParamType(parameterType)})resolver.ResolveOrParameter(typeof({EmitParamType(parameterType)}), \"{parameterName}\", \"{id}\", parameters),");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(id))
                {
                    injectMethodCode.AppendLine($"({EmitParamType(parameterType)})resolver.ResolveOrParameter(typeof({EmitParamType(parameterType)}), \"{parameterName}\", parameters)");
                }
                else
                {
                    injectMethodCode.AppendLine($"({EmitParamType(parameterType)})resolver.ResolveOrParameter(typeof({EmitParamType(parameterType)}), \"{parameterName}\", \"{id}\", parameters)");
                }
            }
        }

        writer.DecreaseIndent();
        writer.DecreaseIndent();
        injectMethodCode.AppendLine(");");
    }

    static string EmitParamType(ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    static string EmitTypeName(ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    static string EmitAccessibility(IFieldSymbol field)
    {
        return $"__x.{field.Name}";
    }

    static string EmitAccessibility(IPropertySymbol property)
    {
        return $"__x.{property.Name}";
    }

    public static bool TryEmitCreateInstanceMethod(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        ReferenceSymbols references,
        in SourceProductionContext context)
    {
        if (typeMeta.ExplicitInjectConstructors.Count > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.MultipleCtorAttributeNotSupported,
                typeMeta.GetLocation(),
                typeMeta.TypeName));
            return false;
        }

        var constructorSymbol = typeMeta.ExplicitInjectConstructors.Count == 1
            ? typeMeta.ExplicitInjectConstructors.First()
            : typeMeta.ExplicitConstructors.OrderByDescending(ctor => ctor.Parameters.Length).FirstOrDefault();

        // Use implicit empty ctor
        constructorSymbol ??= typeMeta.Symbol.InstanceConstructors
            .FirstOrDefault(x => x.IsImplicitlyDeclared && x.Parameters.Length == 0);

        if (constructorSymbol == null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.ConstructorNotFound,
                typeMeta.GetLocation(),
                typeMeta.TypeName));
            return false;
        }

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

        using (codeWriter.BeginBlockScope("public object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters)"))
        {
            if (references.UnityEngineComponent != null &&
                typeMeta.InheritsFrom(references.UnityEngineComponent))
            {
                codeWriter.AppendLine($"throw new NotSupportedException(\"UnityEngine.Component:{typeMeta.TypeName} cannot be `new`\");");
                return true;
            }
            
            if (constructorSymbol.Parameters.Length == 0)
            {
                codeWriter.AppendLine($"var __instance = new {typeMeta.TypeName}();");
            }
            else
            {
                codeWriter.AppendLine($"var __instance = new {typeMeta.TypeName}(");
                codeWriter.IncreasaeIndent();
                
                var parameters = constructorSymbol.Parameters;
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var parameterType = parameter.Type;
                    var parameterName = parameter.Name;
                    
                    string id = null;
                    foreach (var attribute in parameter.GetAttributes())
                    {
                        if (attribute.AttributeClass.Name == "InjectWithIdAttribute" && attribute.ConstructorArguments.Length > 0)
                        {
                            id = attribute.ConstructorArguments[0].Value.ToString();
                            break;
                        }
                    }

                    if (i + 1 < parameters.Length)
                    {
                        if (string.IsNullOrEmpty(id))
                        {
                            codeWriter.AppendLine($"({EmitParamType(parameterType)})resolver.ResolveOrParameter(typeof({EmitParamType(parameterType)}), \"{parameterName}\", parameters),");
                        }
                        else
                        {
                            codeWriter.AppendLine($"({EmitParamType(parameterType)})resolver.ResolveOrParameter(typeof({EmitParamType(parameterType)}), \"{parameterName}\", \"{id}\", parameters),");
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(id))
                        {
                            codeWriter.AppendLine($"({EmitParamType(parameterType)})resolver.ResolveOrParameter(typeof({EmitParamType(parameterType)}), \"{parameterName}\", parameters)");
                        }
                        else
                        {
                            codeWriter.AppendLine($"({EmitParamType(parameterType)})resolver.ResolveOrParameter(typeof({EmitParamType(parameterType)}), \"{parameterName}\", \"{id}\", parameters)");
                        }
                    }
                }
                
                codeWriter.DecreaseIndent();
                codeWriter.AppendLine(");");
            }
            
            codeWriter.AppendLine("Inject(__instance, resolver, parameters);");
            codeWriter.AppendLine("return __instance;");
        }
        return true;
    }
}
