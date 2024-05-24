using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEditor;
using NUnit.Framework;

public class InterceptedEventsPreviewTests
{
    [Test]
    public void InterceptedEventsPreviewCacheUsingTypeCacheReturnsSameTypes()
    {
        var typeCacheEventInterfaces = new List<Type>();
        TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<IEventSystemHandler>();
        foreach (var type in types)
        {
            if (!type.IsInterface)
                continue;

            typeCacheEventInterfaces.Add(type);
        }

        var appDomainEventInterfaces = new List<Type>();
        foreach (var type in GetAccessibleTypesInLoadedAssemblies())
        {
            if (!type.IsInterface)
                continue;

            appDomainEventInterfaces.Add(type);
        }

        Assert.AreNotEqual(typeCacheEventInterfaces.Count, appDomainEventInterfaces.Count, "Did not find the same number of EventInterface types");

        for (int i = 0; i < typeCacheEventInterfaces.Count; ++i)
        {
            Assert.Contains(typeCacheEventInterfaces[i], appDomainEventInterfaces);
        }
    }

    private static IEnumerable<Type> GetAccessibleTypesInLoadedAssemblies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (var i = 0; i < assemblies.Length; ++i)
        {
            Type[] types;
            var assembly = assemblies[i];

            if (assembly == null)
                continue;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                // assembly.GetTypes() might fail in case the Assembly cannot resolve all its references,
                // or in case it was built targetting a newer version of .NET.
                // In case the resolution fails for some types, we can still access the ones that have been
                // properly loaded.
                types = e.Types;
            }

            for (var j = 0; j < types.Length; ++j)
            {
                var type = types[j];
                if (type == null)
                    continue;

                yield return type;
            }
        }
    }
}
