using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.CompilationPipeline.Common.Diagnostics;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;
using VContainer.Internal;

namespace VContainer.Editor.CodeGen
{
    public sealed class InjectionILGenerator
    {
        readonly ModuleDefinition module;
        readonly IList<string> targetNamespaces;

        TypeReference ObjectResolverTypeRef =>
            objectResolverTypeRef ?? (objectResolverTypeRef = module.ImportReference(typeof(IObjectResolver)));

        TypeReference InjectorTypeRef =>
            injectorTypeRef ??
            (injectorTypeRef = module.ImportReference(typeof(IInjector)));

        TypeReference InjectParameterListTypeRef =>
            injectParameterListTypeRef ??
            (injectParameterListTypeRef = module.ImportReference(typeof(IReadOnlyList<IInjectParameter>)));

        MethodReference GetTypeFromHandleRef =>
            getTypeFromHandleRef ??
            (getTypeFromHandleRef = module.ImportReference(typeof(Type).GetMethod("GetTypeFromHandle")));

        MethodReference BaseEmptyConstructorRef =>
            baseEmptyConstructorRef ??
            (baseEmptyConstructorRef = module.ImportReference(typeof(object).GetConstructor(Type.EmptyTypes)));

        MethodReference ResolveMethodRef =>
            resolveMethodRef ??
            (resolveMethodRef =
                module.ImportReference(typeof(IObjectResolverExtensions).GetMethod("ResolveNonGeneric")));

        MethodReference ResolveOrParameterMethodRef =>
            resolveOrParameterMethodRef ??
            (resolveOrParameterMethodRef =
                module.ImportReference(typeof(IObjectResolverExtensions).GetMethod("ResolveOrParameter")));

        TypeReference objectResolverTypeRef;
        TypeReference injectorTypeRef;
        TypeReference injectParameterListTypeRef;
        MethodReference baseEmptyConstructorRef;
        MethodReference getTypeFromHandleRef;
        MethodReference resolveMethodRef;
        MethodReference resolveOrParameterMethodRef;

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

            foreach (var typeDef in module.Types)
            {
                try
                {
                    if (TryGenerate(typeDef, diagnosticMessages))
                    {
                        count += 1;
                    }
                }
                catch (Exception ex)
                {
                    diagnosticMessages.Add(new DiagnosticMessage
                    {
                        DiagnosticType = DiagnosticType.Error,
                        MessageData = $"VContainer failed pre code gen for {typeDef.FullName} : {ex.Message}\n{ex.StackTrace}"
                    });
                    return false;
                }
            }

            sw.Stop();
            if (count > 0)
            {
                var assemblyName = module.Assembly.Name.Name;
                UnityEngine.Debug.Log($"VContainer code generation optimization for {assemblyName} {count} types ({sw.Elapsed.TotalMilliseconds}ms)");
                return true;
            }
            return false;
        }

        bool TryGenerate(TypeDefinition typeDef, List<DiagnosticMessage> diagnosticMessages)
        {
            var type = Type.GetType($"{typeDef.FullName}, {module.Assembly.FullName}");

            if (type == null || !NeedsInjectType(type))
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

            GenerateInnerInjectorType(typeDef, injectTypeInfo);
            return true;
        }

        bool NeedsInjectType(Type type)
            => !type.IsEnum &&
               !type.IsValueType &&
               !type.IsInterface &&
               !(type.IsAbstract && type.IsSealed) &&
               !typeof(Delegate).IsAssignableFrom(type) &&
               !typeof(Attribute).IsAssignableFrom(type) &&
               !type.IsGenericType &&
               (targetNamespaces == null ||
                targetNamespaces.Count <= 0 ||
                targetNamespaces.Contains(type.Namespace));

        void GenerateInnerInjectorType(TypeDefinition typeDef, InjectTypeInfo injectTypeInfo)
        {
            var injectorTypeDef = new TypeDefinition(
                "",
                "__GeneratedInjector",
                TypeAttributes.NestedPrivate | TypeAttributes.Sealed,
                module.TypeSystem.Object);

            var injectorImpl = new InterfaceImplementation(InjectorTypeRef);
            injectorTypeDef.Interfaces.Add(injectorImpl);

            GenerateDefaultConstructor(injectorTypeDef);
            GenerateInjectMethod(typeDef, injectorTypeDef, injectTypeInfo);
            GenerateCreateInstanceMethod(typeDef, injectorTypeDef, injectTypeInfo);
            GenerateInjectorGetterMethod(typeDef, injectorTypeDef);

            typeDef.NestedTypes.Add(injectorTypeDef);
        }

        void GenerateDefaultConstructor(TypeDefinition typeDef)
        {
            var constructorDef = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public |
                MethodAttributes.HideBySig |
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName,
                module.TypeSystem.Void);

            var processor = constructorDef.Body.GetILProcessor();
            // processor.Emit(OpCodes.Nop);
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Call, BaseEmptyConstructorRef);
            processor.Emit(OpCodes.Ret);

            constructorDef.DeclaringType = typeDef;

            typeDef.Methods.Add(constructorDef);
        }

        void GenerateCreateInstanceMethod(
            TypeDefinition typeDef,
            TypeDefinition injectorTypeDef,
            InjectTypeInfo injectTypeInfo)
        {
            var methodDef = new MethodDefinition("CreateInstance",
                MethodAttributes.Public | MethodAttributes.Virtual,
                module.TypeSystem.Object);

            var injectMethodDef = injectorTypeDef.Methods.Single(x => x.Name == "Inject");

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
                var paramTypeRef = Utils.CreateParameterTypeReference(module, paramInfo.ParameterType, typeDef);

                var paramVariableDef = new VariableDefinition(paramTypeRef);
                body.Variables.Add(paramVariableDef);

                // TODO: Add ExceptionHandler
                // Call ResolveOrParameter(IObjectResolver, Type, string, IReadOnlyList<IInjectParameter>)
                processor.Emit(OpCodes.Ldarg_1);
                processor.Emit(OpCodes.Ldtoken, paramTypeRef);
                processor.Emit(OpCodes.Call, GetTypeFromHandleRef);
                processor.Emit(OpCodes.Ldstr, paramInfo.Name);
                processor.Emit(OpCodes.Ldarg_2);
                processor.Emit(OpCodes.Call, ResolveOrParameterMethodRef);
                processor.Emit(OpCodes.Unbox_Any, paramTypeRef);
                processor.Emit(OpCodes.Stloc_S, paramVariableDef);
                processor.Emit(OpCodes.Ldloc_S, paramVariableDef);
            }

            processor.Emit(OpCodes.Newobj, constructorRef);
            processor.Emit(OpCodes.Stloc_0);

            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldloc_0);
            processor.Emit(OpCodes.Ldarg_1);
            processor.Emit(OpCodes.Ldarg_2);
            processor.Emit(OpCodes.Callvirt, injectMethodDef);

            processor.Emit(OpCodes.Ldloc_0);
            processor.Emit(OpCodes.Ret);
        }

        void GenerateInjectMethod(TypeDefinition typeDef, TypeDefinition injectorTypeDef, InjectTypeInfo injectTypeInfo)
        {
            var methodDef = new MethodDefinition("Inject",
                MethodAttributes.Public | MethodAttributes.Virtual,
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
                        var paramTypeRef = Utils.CreateParameterTypeReference(module, paramInfo.ParameterType, typeDef);

                        var paramVariableDef = new VariableDefinition(paramDef.ParameterType);
                        body.Variables.Add(paramVariableDef);

                        // TODO: Add ExceptionHandler
                        processor.Emit(OpCodes.Ldarg_2);
                        processor.Emit(OpCodes.Ldtoken, paramTypeRef);
                        processor.Emit(OpCodes.Call, GetTypeFromHandleRef);
                        processor.Emit(OpCodes.Ldstr, paramInfo.Name);
                        processor.Emit(OpCodes.Ldarg_3);
                        processor.Emit(OpCodes.Call, ResolveOrParameterMethodRef);
                        processor.Emit(OpCodes.Unbox_Any, paramTypeRef);
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
                    var propertyTypeRef = Utils.CreateParameterTypeReference(module, injectProperty.PropertyType, typeDef);

                    processor.Emit(OpCodes.Ldloc_S, instanceVariableDef);

                    // TODO: Add ExceptionHandler
                    // instance.Property = resolver.Resolve(Type)
                    processor.Emit(OpCodes.Ldarg_2);
                    processor.Emit(OpCodes.Ldtoken, propertyTypeRef);
                    processor.Emit(OpCodes.Call, GetTypeFromHandleRef);
                    processor.Emit(OpCodes.Call, ResolveMethodRef);
                    processor.Emit(OpCodes.Callvirt, propertySetterRef);
                }
            }

            if (injectTypeInfo.InjectFields != null)
            {
                foreach (var injectField in injectTypeInfo.InjectFields)
                {
                    var fieldRef = module.ImportReference(injectField);
                    var fieldTypeRef = Utils.CreateParameterTypeReference(module, injectField.FieldType, typeDef);

                    processor.Emit(OpCodes.Ldloc_S, instanceVariableDef);

                    // TODO: Add ExceptionHandler
                    // instance.Field = resolver.Resolve(Type)
                    processor.Emit(OpCodes.Ldarg_2);
                    processor.Emit(OpCodes.Ldtoken, fieldTypeRef);
                    processor.Emit(OpCodes.Call, GetTypeFromHandleRef);
                    processor.Emit(OpCodes.Call, ResolveMethodRef);
                    processor.Emit(OpCodes.Stfld, fieldRef);
                }
            }

            processor.Emit(OpCodes.Ret);
        }

        void GenerateInjectorGetterMethod(TypeDefinition typeDef, TypeDefinition injectorTypeDef)
        {
            var constructorDef = injectorTypeDef.GetConstructors().First();
            var injectorGetterDef = new MethodDefinition(
                "__GetGeneratedInjector",
                MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig,
                InjectorTypeRef);

            var processor = injectorGetterDef.Body.GetILProcessor();
            processor.Emit(OpCodes.Nop);
            processor.Emit(OpCodes.Newobj, constructorDef);
            processor.Emit(OpCodes.Ret);
            typeDef.Methods.Add(injectorGetterDef);
        }
    }
}