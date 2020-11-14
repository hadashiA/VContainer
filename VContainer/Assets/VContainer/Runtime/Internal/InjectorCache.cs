using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace VContainer.Internal
{
    static class InjectorCache
    {
        static readonly ConcurrentDictionary<Type, IInjector> Injectors = new ConcurrentDictionary<Type, IInjector>();

        public static IInjector GetOrBuild(Type type)
        {
            return Injectors.GetOrAdd(type, type1 =>
            {
                var getter = type.GetMethod("__GetGeneratedInjector", BindingFlags.Static | BindingFlags.Public);
                if (getter != null)
                {
                    return (IInjector)getter.Invoke(null, null);
                }

                return ReflectionInjector.Build(type);
            });
        }
    }
}
