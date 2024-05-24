using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal abstract class AttributeFinderBase : IAttributeFinder
    {
        public abstract IEnumerable<Type> Search(ITest tests, ITestFilter filter, RuntimePlatform testTargetPlatform);
    }

    internal interface IAttributeFinder
    {
        IEnumerable<Type> Search(ITest tests, ITestFilter filter, RuntimePlatform testTargetPlatform);
    }

    internal abstract class AttributeFinderBase<T1, T2> : AttributeFinderBase where T2 : Attribute
    {
        private readonly Func<T2, Type> m_TypeSelector;
        protected AttributeFinderBase(Func<T2, Type> typeSelector)
        {
            m_TypeSelector = typeSelector;
        }

        public override IEnumerable<Type> Search(ITest tests, ITestFilter filter, RuntimePlatform testTargetPlatform)
        {
            var selectedTests = new List<ITest>();
            GetMatchingTests(tests, filter, ref selectedTests, testTargetPlatform);

            var result = new List<Type>();
            result.AddRange(GetTypesFromPrebuildAttributes(selectedTests));
            result.AddRange(GetTypesFromInterface(selectedTests, testTargetPlatform));

            return result.Distinct();
        }

        private static void GetMatchingTests(ITest tests, ITestFilter filter, ref List<ITest> resultList, RuntimePlatform testTargetPlatform)
        {
            foreach (var test in tests.Tests)
            {
                if (IsTestEnabledOnPlatform(test, testTargetPlatform))
                {
                    if (test.IsSuite)
                    {
                        GetMatchingTests(test, filter, ref resultList, testTargetPlatform);
                    }
                    else
                    {
                        if (filter.Pass(test))
                            resultList.Add(test);
                    }
                }
            }
        }

        private static bool IsTestEnabledOnPlatform(ITest test, RuntimePlatform testTargetPlatform)
        {
            if (test.Method == null)
            {
                return true;
            }

            var attributesFromMethods = test.Method.GetCustomAttributes<UnityPlatformAttribute>(true);
            var attributesFromTypes = test.Method.TypeInfo.GetCustomAttributes<UnityPlatformAttribute>(true);

            if (attributesFromMethods.Length == 0 && attributesFromTypes.Length == 0)
            {
                return true;
            }

            if (!attributesFromMethods.All(a => a.IsPlatformSupported(testTargetPlatform)))
            {
                return false;
            }

            if (!attributesFromTypes.All(a => a.IsPlatformSupported(testTargetPlatform)))
            {
                return false;
            }

            return true;
        }

        private IEnumerable<Type> GetTypesFromPrebuildAttributes(IEnumerable<ITest> tests)
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            allAssemblies = allAssemblies.Where(x => x.GetReferencedAssemblies().Any(z => z.Name == "UnityEditor.TestRunner")).ToArray();
            var attributesFromAssemblies = allAssemblies.SelectMany(assembly => assembly.GetCustomAttributes(typeof(T2), true).OfType<T2>());
            var attributesFromMethods = tests.SelectMany(t => t.Method.GetCustomAttributes<T2>(true).Select(attribute => attribute));
            var attributesFromTypes = tests.SelectMany(t => t.Method.TypeInfo.GetCustomAttributes<T2>(true).Select(attribute => attribute));

            var result = new List<T2>();
            result.AddRange(attributesFromAssemblies);
            result.AddRange(attributesFromMethods);
            result.AddRange(attributesFromTypes);

            return result.Select(m_TypeSelector).Where(type => type != null);
        }

        private static IEnumerable<Type> GetTypesFromInterface(IEnumerable<ITest> selectedTests, RuntimePlatform testTargetPlatform)
        {
            var typesWithInterfaces = selectedTests.Where(t => typeof(T1).IsAssignableFrom(t.Method.TypeInfo.Type) && IsTestEnabledOnPlatform(t, testTargetPlatform));
            return typesWithInterfaces.Select(t => t.Method.TypeInfo.Type);
        }
    }
}
