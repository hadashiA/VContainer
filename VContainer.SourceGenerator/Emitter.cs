using System.Collections.Generic;
using System.Linq;

namespace VContainer.SourceGenerator;

static class Emitter
{
    public static bool TryEmitGeneratedInjector(
        InjectorModel model,
        CodeWriter codeWriter,
        List<DiagnosticInfo> diagnostics)
    {
        if (model.IsNested)
        {
            if (model.ExplicitInjectable)
            {
                diagnostics.Add(new DiagnosticInfo(
                    DiagnosticDescriptors.NestedNotSupported,
                    model.Location,
                    model.SymbolName));
            }
            return false;
        }

        if (model.IsAbstract)
        {
            if (model.ExplicitInjectable)
            {
                diagnostics.Add(new DiagnosticInfo(
                    DiagnosticDescriptors.AbstractNotAllow,
                    model.Location,
                    model.TypeName));
            }
            return false;
        }

        if (model.IsGenerics)
        {
            return false; // TODO:
        }

        codeWriter.AppendLine("using VContainer;");
        codeWriter.AppendLine();

        var hasNamespace = model.Namespace.Length > 0;
        if (hasNamespace)
        {
            codeWriter.AppendLine($"namespace {model.Namespace}");
            codeWriter.BeginBlock();
        }

        var typeName = model.TypeName
            .Replace("global::", "")
            .Replace("<", "_")
            .Replace(">", "_");

        var generateTypeName = $"{typeName}GeneratedInjector";

        codeWriter.AppendLine("[global::VContainer.Preserve]");
        using (codeWriter.BeginBlockScope($"class {generateTypeName} : global::VContainer.IInjector"))
        {
            codeWriter.AppendLine();
            if (!TryEmitCreateInstanceMethod(model, codeWriter, diagnostics))
            {
                return false;
            }

            codeWriter.AppendLine();

            if (!TryEmitInjectMethod(model, codeWriter, diagnostics))
            {
                return false;
            }
        }

        if (hasNamespace)
        {
            codeWriter.EndBlock();
        }

        return true;
    }

    static bool TryEmitInjectMethod(
        InjectorModel model,
        CodeWriter codeWriter,
        List<DiagnosticInfo> diagnostics)
    {
        using (codeWriter.BeginBlockScope(
                   "public void Inject(object instance, global::VContainer.IObjectResolver resolver, global::System.Collections.Generic.IReadOnlyList<global::VContainer.IInjectParameter> parameters)"))
        {
            if (model.InjectFields.Count <= 0 &&
                model.InjectProperties.Count <= 0 &&
                model.InjectMethods.Count <= 0)
            {
                codeWriter.AppendLine("return;");
                return true;
            }

            codeWriter.AppendLine($"var __x = ({model.TypeName})instance;");

            var error = false;

            // verify field
            foreach (var field in model.InjectFields)
            {
                if (!field.CanBeSet)
                {
                    diagnostics.Add(new DiagnosticInfo(
                        DiagnosticDescriptors.PrivateFieldNotSupported,
                        field.Location ?? model.Location,
                        field.Name));
                    error = true;
                }

                if (field.IsTypeParameter)
                {
                    diagnostics.Add(new DiagnosticInfo(
                        DiagnosticDescriptors.GenericsNotSupported,
                        field.Location ?? model.Location,
                        field.Name));
                    error = true;
                }
            }

            // verify property
            foreach (var prop in model.InjectProperties)
            {
                if (!prop.CanBeSet)
                {
                    diagnostics.Add(new DiagnosticInfo(
                        DiagnosticDescriptors.PrivatePropertyNotSupported,
                        prop.Location ?? model.Location,
                        prop.Name));
                    error = true;
                }

                if (prop.IsTypeParameter)
                {
                    diagnostics.Add(new DiagnosticInfo(
                        DiagnosticDescriptors.GenericsNotSupported,
                        prop.Location ?? model.Location,
                        prop.Name));
                    error = true;
                }
            }

            // verify method
            if (model.InjectMethods.Count > 1)
            {
                var first = model.InjectMethods.First();
                diagnostics.Add(new DiagnosticInfo(
                    DiagnosticDescriptors.MultipleInjectMethodNotSupported,
                    first.Location ?? model.Location,
                    first.Name));
                error = true;
            }

            foreach (var method in model.InjectMethods)
            {
                if (!method.CanBeCalled)
                {
                    diagnostics.Add(new DiagnosticInfo(
                        DiagnosticDescriptors.PrivateMethodNotSupported,
                        method.Location ?? model.Location,
                        method.Name));
                    error = true;
                }
                if (method.IsGeneric)
                {
                    diagnostics.Add(new DiagnosticInfo(
                        DiagnosticDescriptors.GenericsNotSupported,
                        method.Location ?? model.Location,
                        method.Name));
                    error = true;
                }
            }

            if (error)
            {
                return false;
            }

            foreach (var field in model.InjectFields)
            {
                EmitMemberInjection(codeWriter, field);
            }

            foreach (var prop in model.InjectProperties)
            {
                EmitMemberInjection(codeWriter, prop);
            }

            foreach (var method in model.InjectMethods)
            {
                EmitParameterizedMethodCall(codeWriter, method);
            }
            return true;
        }
    }

    static void EmitMemberInjection(CodeWriter codeWriter, MemberModel member)
    {
        codeWriter.AppendLine($"__x.{member.Name} = ({member.TypeFullName})resolver.ResolveOrParameter(typeof({member.TypeFullName}), \"{member.Name}\", parameters, {member.KeyLiteral});");
    }

    static string GenerateParameterInjectionCode(ParameterModel parameter, bool includeComma = false)
    {
        var code = $"({parameter.TypeFullName})resolver.ResolveOrParameter(typeof({parameter.TypeFullName}), \"{parameter.Name}\", parameters, {parameter.KeyLiteral})";
        if (includeComma)
            code += ",";
        return code;
    }

    static void EmitParameterizedMethodCall(CodeWriter codeWriter, MethodModel method)
    {
        var parameterVariableNames = new List<string>();
        var methodAccess = method.IsStatic ? method.ContainingTypeFullName : "__x";

        using (codeWriter.BeginBlockScope())
        {
            // Generate local variables for parameters
            foreach (var parameter in method.Parameters)
            {
                var parameterVariableName = "param_" + parameter.Name;
                parameterVariableNames.Add(parameterVariableName);

                var injectionCode = GenerateParameterInjectionCode(parameter);
                codeWriter.AppendLine($"var {parameterVariableName} = {injectionCode};");
            }

            // Call the method with the parameters
            codeWriter.AppendLine(!method.ReturnsVoid
                ? $"var result = {methodAccess}.{method.Name}({string.Join(", ", parameterVariableNames)});"
                : $"{methodAccess}.{method.Name}({string.Join(", ", parameterVariableNames)});");
        }
    }

    public static bool TryEmitCreateInstanceMethod(
        InjectorModel model,
        CodeWriter codeWriter,
        List<DiagnosticInfo> diagnostics)
    {
        if (model.HasMultipleInjectConstructors)
        {
            diagnostics.Add(new DiagnosticInfo(
                DiagnosticDescriptors.MultipleCtorAttributeNotSupported,
                model.Location,
                model.TypeName));
            return false;
        }

        if (!model.ConstructorFound)
        {
            diagnostics.Add(new DiagnosticInfo(
                DiagnosticDescriptors.ConstructorNotFound,
                model.Location,
                model.TypeName));
            return false;
        }

        if (!model.ConstructorCanBeCalled)
        {
            diagnostics.Add(new DiagnosticInfo(
                DiagnosticDescriptors.PrivateConstructorNotSupported,
                model.Location,
                model.TypeName));
            return false;
        }

        if (model.ConstructorIsGeneric)
        {
            diagnostics.Add(new DiagnosticInfo(
                DiagnosticDescriptors.GenericsNotSupported,
                model.Location,
                model.TypeName));
            return false;
        }

        using (codeWriter.BeginBlockScope("public object CreateInstance(global::VContainer.IObjectResolver resolver, global::System.Collections.Generic.IReadOnlyList<global::VContainer.IInjectParameter> parameters)"))
        {
            // Handle Unity components - they shouldn't be instantiated with 'new'
            if (model.IsUnityComponent)
            {
                codeWriter.AppendLine($"throw new global::System.NotSupportedException(\"UnityEngine.Component:{model.TypeName} cannot be `new`\");");
                return true;
            }

            // Handle parameterless constructor
            if (model.ConstructorParameters.Count == 0)
            {
                codeWriter.AppendLine($"var __instance = new {model.TypeName}();");
            }
            else
            {
                codeWriter.AppendLine($"var __instance = new {model.TypeName}(");
                codeWriter.IncreasaeIndent();

                // Generate parameter list
                var parameters = model.ConstructorParameters;
                for (var i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    bool isLastParameter = (i + 1 >= parameters.Count);
                    var injectionCode = GenerateParameterInjectionCode(parameter, !isLastParameter);
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
