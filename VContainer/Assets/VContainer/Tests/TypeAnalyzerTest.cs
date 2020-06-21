using System.Reflection;
using NUnit.Framework;
using VContainer.Internal;

namespace VContainer.Tests
{
    [TestFixture]
    public class TypeAnalyzerTest
    {
        class HasNoConstructor
        {
        }

        class HasNoAttributeConstructor
        {
            public HasNoAttributeConstructor(int x)
            {
            }

            public HasNoAttributeConstructor(int x, int y)
            {
            }
        }

        class HasInjectConstructor
        {
            [Inject]
            public HasInjectConstructor(int x, int y)
            {
            }
        }

        class HasMultipleInjectConstructor
        {
            [Inject]
            public HasMultipleInjectConstructor(int x)
            {
            }

            public HasMultipleInjectConstructor(int x, int y)
            {
            }
        }

        [Test]
        public void Analyze()
        {
            {
                var injectTypeInfo = TypeAnalyzer.Analyze(typeof(HasNoConstructor));
                Assert.That(injectTypeInfo.InjectConstructor.ParameterInfos.Length, Is.EqualTo(0));
            }

            {
                var injectTypeInfo = TypeAnalyzer.Analyze(typeof(HasNoAttributeConstructor));
                Assert.That(injectTypeInfo.InjectConstructor.ParameterInfos.Length, Is.EqualTo(2));
            }
            {
                var injectTypeInfo = TypeAnalyzer.Analyze(typeof(HasInjectConstructor));
                // Assert.That(injectTypeInfo.InjectConstructor.ConstructorInfo.GetCustomAttribute<InjectAttribute>(), Is.Not.Null);
            }
        }
    }
}
