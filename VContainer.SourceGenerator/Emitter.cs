using System;
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

            if (!TryEmitInjectMethod(typeMeta, codeWriter, references, in context))
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
        ReferenceSymbols references,
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
                EmitFieldInjection(codeWriter, fieldSymbol, references);
            }

            foreach (var propSymbol in typeMeta.InjectProperties)
            {
                EmitPropertyInjection(codeWriter, propSymbol, references);
            }

            foreach (var methodSymbol in typeMeta.InjectMethods)
            {
                EmitParameterizedMethodCall(codeWriter, methodSymbol, references);
            }
            return true;
        }
    }

    /// <summary>
    /// Extracts the ID object from an Inject attribute if present on the symbol
    /// </summary>
    /// <param name="symbol">The symbol to check for attributes</param>
    /// <param name="references">The reference symbols containing the InjectAttribute type</param>
    /// <returns>The ID object if the attribute is present with a value, otherwise null</returns>
    private static object? ExtractIdFromInjectAttribute(ISymbol symbol, ReferenceSymbols references)
    {   
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass == null)
                continue;
                
            // Check if this is an InjectAttribute using symbol comparison
            var isInjectAttribute = SymbolEqualityComparer.Default.Equals(
                attribute.AttributeClass, 
                references.VContainerInjectAttribute);

            if (!isInjectAttribute || attribute.ConstructorArguments.Length <= 0)
            {
                continue;
            }
            
            var constructorArg = attribute.ConstructorArguments[0];

            // For enum values, return the TypedConstant to preserve type information
            return constructorArg.Kind == TypedConstantKind.Enum 
                ? constructorArg 
                : constructorArg.Value;
        }
        
        return null;
    }

    private static void EmitMemberInjection(CodeWriter codeWriter, ISymbol memberSymbol, ITypeSymbol memberType, string memberName, ReferenceSymbols references)
    {
        var id = ExtractIdFromInjectAttribute(memberSymbol, references);

        codeWriter.AppendLine($"__x.{memberName} = ({EmitParamType(memberType)})resolver.ResolveOrParameter(typeof({EmitParamType(memberType)}), \"{memberName}\", parameters, {EmitIdValue(id)});");
    }

    private static void EmitFieldInjection(CodeWriter codeWriter, IFieldSymbol field, ReferenceSymbols references)
    {
        EmitMemberInjection(codeWriter, field, field.Type, field.Name, references);
    }

    private static void EmitPropertyInjection(CodeWriter codeWriter, IPropertySymbol property, ReferenceSymbols references)
    {
        EmitMemberInjection(codeWriter, property, property.Type, property.Name, references);
    }

    private static string GenerateParameterInjectionCode(IParameterSymbol parameter, ReferenceSymbols references, bool includeComma = false)
    {
        var parameterType = parameter.Type;
        var parameterName = parameter.Name;
        
        var id = ExtractIdFromInjectAttribute(parameter, references);

        var code = $"({EmitParamType(parameterType)})resolver.ResolveOrParameter(typeof({EmitParamType(parameterType)}), \"{parameterName}\", parameters, {EmitIdValue(id)})";
        
        if (includeComma)
            code += ",";
            
        return code;
    }

    private static object EmitIdValue(object? id)
    {
        return id switch
        {
            null => "null",
            string str => $"\"{str}\"",
            bool b => b ? bool.TrueString : bool.FalseString,
            TypedConstant { Kind: TypedConstantKind.Enum, Type: not null } tc => EnumToStringRepresentation(tc),
            TypedConstant { Kind: TypedConstantKind.Primitive, Value: string strVal } => EmitIdValue(strVal),
            TypedConstant { Value: not null } tc => tc.Value.ToString(),
            _ => id.ToString()
        };
    }

    private static void EmitParameterizedMethodCall(CodeWriter codeWriter, IMethodSymbol methodSymbol, ReferenceSymbols references)
    {
        var parameters = methodSymbol.Parameters;
        var parameterVariableNames = new List<string>();
        
        var methodName = methodSymbol.Name;
        var methodAccess = methodSymbol.IsStatic ? $"{EmitTypeName(methodSymbol.ContainingType)}" : "__x";

        using (codeWriter.BeginBlockScope())
        {
            // Generate local variables for parameters
            foreach (var parameter in parameters)
            {
                var parameterName = parameter.Name;
                var parameterVariableName = "param_" + parameterName;
                parameterVariableNames.Add(parameterVariableName);
                
                var injectionCode = GenerateParameterInjectionCode(parameter, references);
                codeWriter.AppendLine($"var {parameterVariableName} = {injectionCode};");
            }

            // Call the method with the parameters
            codeWriter.AppendLine(!methodSymbol.ReturnsVoid
                ? $"var result = {methodAccess}.{methodName}({string.Join(", ", parameterVariableNames)});"
                : $"{methodAccess}.{methodName}({string.Join(", ", parameterVariableNames)});");
        }
    }
    
    private static string EmitParamType(ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static string EmitTypeName(ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    private static string EnumToStringRepresentation(TypedConstant tc)
    {
        return $"({tc.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}){tc.Value}";
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
            // Handle Unity components - they shouldn't be instantiated with 'new'
            if (references.UnityEngineComponent != null &&
                typeMeta.InheritsFrom(references.UnityEngineComponent))
            {
                codeWriter.AppendLine($"throw new NotSupportedException(\"UnityEngine.Component:{typeMeta.TypeName} cannot be `new`\");");
                return true;
            }
            
            // Handle parameterless constructor
            if (constructorSymbol.Parameters.Length == 0)
            {
                codeWriter.AppendLine($"var __instance = new {typeMeta.TypeName}();");
            }
            else
            {
                codeWriter.AppendLine($"var __instance = new {typeMeta.TypeName}(");
                codeWriter.IncreasaeIndent();
                
                // Generate parameter list
                var parameters = constructorSymbol.Parameters;
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    bool isLastParameter = (i + 1 >= parameters.Length);
                    var injectionCode = GenerateParameterInjectionCode(parameter, references, !isLastParameter);
                    codeWriter.AppendLine(injectionCode);
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

