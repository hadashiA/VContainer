using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace VContainer.Internal
{
    static class InjectorCache
    {
        static readonly ConcurrentDictionary<Type, IInjector> Injectors = new ConcurrentDictionary<Type, IInjector>();

        public static void Register<T>(IInjector injector)
        {
            Injectors.AddOrUpdate(typeof(T), injector, (key, existsValue) => injector);
        }

        public static IInjector GetOrBuild(Type type)
        {
            return Injectors.GetOrAdd(type, key =>
            {
                var generatedType = type.Assembly.GetType($"{key.Name}VContainerGeneratedInjector");
                if (generatedType != null)
                {
                    return (IInjector)Activator.CreateInstance(generatedType);
                }

                var getter = key.GetMethod("__GetGeneratedInjector", BindingFlags.Static | BindingFlags.Public);
                if (getter != null)
                {
                    return (IInjector)getter.Invoke(null, null);
                }
                return ReflectionInjector.Build(key);
            });
        }
    }
}
