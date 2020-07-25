using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace VContainer.Internal
{
    public static class InjectorCache
    {
        static readonly ConcurrentDictionary<Type, IInjector> Injectors = new ConcurrentDictionary<Type, IInjector>();

        public static IInjector GetOrBuild(Type type)
        {
            return Injectors.GetOrAdd(type, type1 =>
            {
                var field = type.GetField("__GetGeneratedInjector", BindingFlags.Static | BindingFlags.Public);
                if (field != null)
                {
                    return (IInjector)field.GetValue(null);
                }

                return ReflectionInjector.Build(type);
            });
        }
    }
}