using NUnit.Framework;

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

            [Inject]
            public HasMultipleInjectConstructor(int x, int y)
            {
            }
        }

        [Test]
        public void Analyze()
        {
            // Assert.Throws<VContainerException>(() => TypeAnalyzer);
        }
    }
}