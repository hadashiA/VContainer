using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using VContainer.Internal;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace VContainer.Editor.CodeGen
{
    public sealed class InjectionILGenerator
    {
        readonly ModuleDefinition module;
        readonly IList<string> targetNamespaces;

        TypeReference ObjectResolverTypeRef =>
            objectResolverTypeRef ?? (objectResolverTypeRef = module.ImportReference(typeof(IObjectResolver)));

        TypeReference InjectorTypeRef =>
            injectorTypeRef ?? (injectorTypeRef = module.ImportReference(typeof(IInjector)));

        TypeReference InjectParameterListTypeRef =>
            injectParameterListTypeRef ?? (injectParameterListTypeRef = module.ImportReference(typeof(IReadOnlyList<IInjectParameter>)));

        MethodInfo ResolveMethodInfo =>
            resolveMethodInfo ?? (resolveMethodInfo = typeof(ObjectResolverExtensions).GetMethod("Resolve"));


        TypeReference objectResolverTypeRef;
        TypeReference injectorTypeRef;
        TypeReference injectParameterListTypeRef;
        MethodInfo resolveMethodInfo;

        public InjectionILGenerator(
            ModuleDefinition module,
            IList<string> targetNamespaces)
        {
            this.module = module;
            this.targetNamespaces = targetNamespaces;
        }

        public bool TryGenerate(out List<DiagnosticMessage> diagnosticMessages)
        {
            var count = 0;
            var sw = new Stopwatch();
            sw.Start();

            diagnosticMessages = new List<DiagnosticMessage>();

            try
            {
                foreach (var typeDefinition in module.Types)
                {
                    var changed = TryGenerate(typeDefinition, diagnosticMessages);
                    if (changed)
                    {
                        UnityEngine.Debug.Log($"Type={typeDefinition.Name}");
                        count += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                diagnosticMessages.Add(new DiagnosticMessage
                {
                    DiagnosticType = DiagnosticType.Error,
                    MessageData = $"{ex.Message}\n{ex.StackTrace}"
                });
            }

            sw.Stop();

            if (count > 0)
            {
                UnityEngine.Debug.Log($"VContainer code generation optimization for {count} types ({sw.Elapsed.TotalMilliseconds}ms)");
                return true;
            }
            return false;
        }

        bool TryGenerate(TypeDefinition typeDef, List<DiagnosticMessage> diagnosticMessages)
        {
            var type = Type.GetType($"{typeDef.FullName}, {module.Assembly.FullName}");

            if (!NeedsInjectType(type))
                return false;

            InjectTypeInfo injectTypeInfo;
            try
            {
                injectTypeInfo = TypeAnalyzer.Analyze(type);
            }
            catch (Exception ex)
            {
                diagnosticMessages.Add(new DiagnosticMessage
                {
                    DiagnosticType = DiagnosticType.Warning,
                    MessageData = $"Failed to analyze {type.FullName} : {ex.Message}"
                });
                return false;
            }

            var injectorTypeDef = new TypeDefinition(
                "",
                "__GeneratedInjector",
                TypeAttributes.NestedPrivate | TypeAttributes.Sealed,
                module.TypeSystem.Object);

            var injectorImpl = new InterfaceImplementation(InjectorTypeRef);
            injectorTypeDef.Interfaces.Add(injectorImpl);

            GenerateCreateInstanceMethod(typeDef, injectorTypeDef, injectTypeInfo);
            GenerateInjectMethod(typeDef, injectorTypeDef, injectTypeInfo);

            typeDef.NestedTypes.Add(injectorTypeDef);

            return true;
        }

        bool NeedsInjectType(Type type)
            => !type.IsEnum &&
               !type.IsValueType &&
               !type.IsInterface &&
               !(type.IsAbstract && type.IsSealed) &&
               !typeof(Delegate).IsAssignableFrom(type) &&
               !typeof(Attribute).IsAssignableFrom(type);

        void GenerateCreateInstanceMethod(
            TypeDefinition typeDef,
            TypeDefinition injectorTypeDef,
            InjectTypeInfo injectTypeInfo)
        {
            var methodDef = new MethodDefinition(
                "CreateInstance",
                MethodAttributes.Public,
                module.TypeSystem.Object);

            injectorTypeDef.Methods.Add(methodDef);

            methodDef.Parameters.Add(new ParameterDefinition(ObjectResolverTypeRef)
            {
                Name = "resolver"
            });

            methodDef.Parameters.Add(new ParameterDefinition(InjectParameterListTypeRef)
            {
                Name = "parameters"
            });

            var body = methodDef.Body;
            var processor = body.GetILProcessor();

            processor.Emit(OpCodes.Nop);

            if (injectTypeInfo.InjectConstructor == null ||
                injectTypeInfo.Type.IsSubclassOf(typeof(UnityEngine.Component)))
            {
                processor.Emit(OpCodes.Ldnull);
                processor.Emit(OpCodes.Ret);
                return;
            }

            var resultVariableDef = new VariableDefinition(module.TypeSystem.Object);
            body.Variables.Add(resultVariableDef);

            var constructorRef = module.ImportReference(injectTypeInfo.InjectConstructor.ConstructorInfo);
            for (var i = 0; i < constructorRef.Parameters.Count; i++)
            {
                var paramDef = constructorRef.Parameters[i];
                var paramInfo = injectTypeInfo.InjectConstructor.ParameterInfos[i];
                var resolveMethodRef = module.ImportReference(ResolveMethodInfo.MakeGenericMethod(paramInfo.ParameterType));

                var paramVariableDef = new VariableDefinition(paramDef.ParameterType);
                body.Variables.Add(paramVariableDef);

                // TODO: Add ExceptionHandler
                var resolveStart = processor.Create(OpCodes.Ldarg_1);
                processor.Append(resolveStart);
                processor.Emit(OpCodes.Callvirt, resolveMethodRef);
                processor.Emit(OpCodes.Stloc_S, paramVariableDef);
                processor.Emit(OpCodes.Ldloc_S, paramVariableDef);
            }

            processor.Emit(OpCodes.Newobj, constructorRef);
            processor.Emit(OpCodes.Stloc, resultVariableDef);
            processor.Emit(OpCodes.Ldloc, resultVariableDef);
            processor.Emit(OpCodes.Ret);
        }

        void GenerateInjectMethod(
            TypeDefinition typeDef,
            TypeDefinition injectorTypeDef,
            InjectTypeInfo injectTypeInfo)
        {
            var methodDef = new MethodDefinition(
                "Inject",
                MethodAttributes.Public,
                module.TypeSystem.Void);

            injectorTypeDef.Methods.Add(methodDef);

            methodDef.Parameters.Add(new ParameterDefinition(module.TypeSystem.Object)
            {
                Name = "instance"
            });

            methodDef.Parameters.Add(new ParameterDefinition(ObjectResolverTypeRef)
            {
                Name = "resolver"
            });

            methodDef.Parameters.Add(new ParameterDefinition(InjectParameterListTypeRef)
            {
                Name = "parameters"
            });

            var body = methodDef.Body;
            var processor = body.GetILProcessor();

            processor.Emit(OpCodes.Nop);

            var instanceVariableDef = new VariableDefinition(typeDef);
            body.Variables.Add(instanceVariableDef);

            if (injectTypeInfo.InjectMethods != null ||
                injectTypeInfo.InjectFields != null ||
                injectTypeInfo.InjectProperties != null)
            {
                processor.Emit(OpCodes.Ldarg_1);
                processor.Emit(OpCodes.Unbox_Any, typeDef);
                processor.Emit(OpCodes.Stloc_S, instanceVariableDef);
            }

            if (injectTypeInfo.InjectMethods != null)
            {
                foreach (var injectMethod in injectTypeInfo.InjectMethods)
                {
                    var injectMethodRef = module.ImportReference(injectMethod.MethodInfo);
                    processor.Emit(OpCodes.Ldloc_S, instanceVariableDef);
                    for (var i = 0; i < injectMethodRef.Parameters.Count; i++)
                    {
                        var paramDef = injectMethodRef.Parameters[i];
                        var paramInfo = injectMethod.ParameterInfos[i];

                        var resolveMethodRef = module.ImportReference(ResolveMethodInfo.MakeGenericMethod(paramInfo.ParameterType));

                        var paramVariableDef = new VariableDefinition(paramDef.ParameterType);
                        body.Variables.Add(paramVariableDef);

                        // TODO: Add ExceptionHandler
                        var resolveStart = processor.Create(OpCodes.Ldarg_2);
                        processor.Append(resolveStart);
                        processor.Emit(OpCodes.Callvirt, resolveMethodRef);
                        processor.Emit(OpCodes.Stloc_S, paramVariableDef);
                        processor.Emit(OpCodes.Ldloc_S, paramVariableDef);
                    }
                    processor.Emit(OpCodes.Callvirt, injectMethodRef);
                }
            }

            if (injectTypeInfo.InjectProperties != null)
            {
                foreach (var injectProperty in injectTypeInfo.InjectProperties)
                {
                    var propertySetterRef = module.ImportReference(injectProperty.SetMethod);
                    var resolveMethodRef = module.ImportReference(ResolveMethodInfo.MakeGenericMethod(injectProperty.PropertyType));

                    processor.Emit(OpCodes.Ldloc_S, instanceVariableDef);

                    // TODO: Add ExceptionHandler
                    var resolveStart = processor.Create(OpCodes.Ldarg_2);
                    processor.Append(resolveStart);
                    processor.Emit(OpCodes.Callvirt, resolveMethodRef);
                    processor.Emit(OpCodes.Callvirt, propertySetterRef);
                }
            }

            if (injectTypeInfo.InjectFields != null)
            {
                foreach (var injectField in injectTypeInfo.InjectFields)
                {
                    var injectFieldRef = module.ImportReference(injectField);
                    var resolveMethodRef = module.ImportReference(ResolveMethodInfo.MakeGenericMethod(injectField.FieldType));

                    processor.Emit(OpCodes.Ldloc_S, instanceVariableDef);

                    // TODO: Add ExceptionHandler
                    var resolveStart = processor.Create(OpCodes.Ldarg_2);
                    processor.Append(resolveStart);
                    processor.Emit(OpCodes.Callvirt, resolveMethodRef);
                    processor.Emit(OpCodes.Stfld, injectFieldRef);
                }
            }

            processor.Emit(OpCodes.Ret);
        }
  }
}