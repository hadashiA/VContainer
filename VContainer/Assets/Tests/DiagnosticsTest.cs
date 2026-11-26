using NUnit.Framework;
using VContainer.Diagnostics;

namespace VContainer.Tests
{
    [TestFixture]
    public class DiagnosticsTest
    {
        [Test]
        public void ResolveParentDuringChildScopeConfiguration()
        {
            // Create the child collector first so that it is enumerated before the parent's.
            // While the child scope is being configured, its registrations have no ResolveInfo yet.
            DiagnositcsContext.GetCollector("Child");

            var builder = new ContainerBuilder
            {
                Diagnostics = DiagnositcsContext.GetCollector("Parent"),
            };

            builder.Register<NoDependencyServiceA>(Lifetime.Singleton);
            var container = builder.Build();

            Assert.DoesNotThrow(() => container.CreateScope(childBuilder =>
            {
                childBuilder.Diagnostics = DiagnositcsContext.GetCollector("Child");
                childBuilder.Register<NoDependencyServiceA>(Lifetime.Singleton);

                // Resolving from the parent before the child scope is built used to throw NullReferenceException.
                container.Resolve<NoDependencyServiceA>();
            }));
        }

        [TearDown]
        public void TearDown()
        {
            DiagnositcsContext.RemoveCollector("Parent");
            DiagnositcsContext.RemoveCollector("Child");
        }
    }
}