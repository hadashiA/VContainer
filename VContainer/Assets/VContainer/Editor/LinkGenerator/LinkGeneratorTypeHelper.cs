using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using UnityEngine;
using VContainer.Internal;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace VContainer.Editor.LinkGenerator
{
    internal class LinkGeneratorTypeHelper
    {
        public static void Generate(StreamWriter stream, List<Assembly> assemblies)
        {
            var membersToPreserve = GetTypesFromLifetimeScopesInAsseblies(assemblies);

            if (membersToPreserve == null)
                return;

            var xmlBuilder = new VContainerXmlLinkBuilder();

            foreach (InjectTypeInfo injectTypeInfo in membersToPreserve.Select(TypeAnalyzer.Analyze))
                xmlBuilder.Add(injectTypeInfo);

            xmlBuilder.WriteTo(XmlWriter.Create(stream, new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true }));
        }

        static List<Type> GetTypesFromLifetimeScopesInAsseblies(List<Assembly> assemblies)
        {
            var scopesContainer = new GameObject("[DELETE ME] VContainer.LinkGenerator");
            var scopeInstances = new List<LifetimeScope>();
            var scriptableInstances = new List<ScriptableObject>();
            var typesToPreserve = new List<Type>();

            try {
                scopeInstances.AddRange(assemblies
                    .SelectMany(assembly => assembly.DefinedTypes)
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(LifetimeScope)))
                    .Select(type => (LifetimeScope)scopesContainer.AddComponent(type)));

                var builder = new ContainerBuilder();

                for (var scopeIdx = 0; scopeIdx < scopeInstances.Count; scopeIdx++) {
                    LifetimeScope scope = scopeInstances[scopeIdx];
                    TypeInfo scopeTypeInfo = scope.GetType().GetTypeInfo();

                    MethodInfo configureMethod = scopeTypeInfo.GetDeclaredMethod("Configure");

                    if (configureMethod == null) {
                        Debug.LogWarning(
                            $"[{nameof(LinkGeneratorTypeHelper)}] couldn't generate link.xml for class {scope.GetType().FullName}," +
                            " because we couldn't find method 'Configure'");
                        continue;
                    }

                    // TODO: recursive field check
                    // TODO: serialize reference support
                    // TODO: recursive initialization. Sometimes user can bind field in serializable class
                    foreach (FieldInfo field in scopeTypeInfo.DeclaredFields) {
                        if (field.GetValue(scope) != null)
                            continue;

                        Type fieldType = field.FieldType.GetTypeInfo();

                        if (fieldType.IsAbstract || fieldType.IsInterface || fieldType.IsGenericType)
                            continue;

                        if (fieldType.IsSubclassOf(typeof(Component))) {
                            field.SetValue(scope, scopesContainer.AddComponent(fieldType));
                            continue;
                        }

                        if (fieldType.IsSubclassOf(typeof(ScriptableObject))) {
                            var instance = ScriptableObject.CreateInstance(fieldType);
                            scriptableInstances.Add(instance);
                            field.SetValue(scope, instance);
                            continue;
                        }

                        object fValue = Activator.CreateInstance(fieldType);
                        field.SetValue(scope, fValue);
                    }

                    // TODO: skip full class adding if is error
                    UniversalResult invokeResult = configureMethod.SafeInvoke(scope, new object[] { builder });

                    if (!invokeResult.IsSuccess) {
                        Debug.LogWarning(
                            $"[{nameof(LinkGeneratorTypeHelper)}] couldn't generate link.xml for class {scope.GetType().FullName}," +
                            $" please add dependencies to preserve manually. Exception: {invokeResult.Ex}");
                    }
                }

                for (int typeIdx = 0; typeIdx < builder.Count; typeIdx++)
                    typesToPreserve.Add(builder.registrationBuilders[typeIdx].ImplementationType);
            }
            catch (Exception e) {
                Debug.LogError($"FATAL ERROR: link.xml generation failed with message: {e}");
                return null;
            }
            finally {
                Object.DestroyImmediate(scopesContainer);

                foreach (ScriptableObject o in scriptableInstances)
                    Object.DestroyImmediate(o);
            }

            return typesToPreserve;
        }
    }

    internal static class ReflectionExtension
    {
        public static UniversalResult SafeInvoke(this MethodInfo methodInfo, object instance, object[] @params)
        {
            try {
                methodInfo.Invoke(instance, @params);
            }
            catch (Exception e) {
                return UniversalResult.Failure(e);
            }
            return UniversalResult.Success();
        }
    }

    // TODO make it generic and return for the whole class
    internal readonly ref struct UniversalResult
    {
        public readonly bool IsSuccess;
        public readonly Exception Ex;

        private UniversalResult(bool isSuccess, Exception ex)
        {
            IsSuccess = isSuccess;
            Ex = ex;
        }

        public static UniversalResult Success() => new(true, null);
        public static UniversalResult Failure(Exception ex) => new(false, ex);
    }
}