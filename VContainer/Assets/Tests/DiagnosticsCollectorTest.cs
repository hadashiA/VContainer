using System.Linq;
using NUnit.Framework;
using VContainer.Diagnostics;

namespace VContainer.Tests
{
    [TestFixture]
    public class DiagnosticsCollectorTest
    {
        [TearDown]
        public void TearDown()
        {
            DiagnositcsContext.RemoveCollector("Root");
            DiagnositcsContext.RemoveCollector("Child");
        }

        [Test]
        public void DisposedChildScopeDoesNotAddInstancesToRootDiagnostics()
        {
            var builder = new ContainerBuilder
            {
                Diagnostics = DiagnositcsContext.GetCollector("Root")
            };
            builder.Register<NoDependencyServiceA>(Lifetime.Scoped);

            using (var root = builder.Build())
            {
                root.Resolve<NoDependencyServiceA>();

                using (var child = root.CreateScope(childBuilder =>
                           childBuilder.Diagnostics = DiagnositcsContext.GetCollector("Child")))
                {
                    var instance = child.Resolve<NoDependencyServiceA>();
                    Assert.That(child.Resolve<NoDependencyServiceA>(), Is.SameAs(instance));

                    // The instance created by the child scope is tracked by the child's collector.
                    var childInfo = DiagnositcsContext.GetCollector("Child").GetDiagnosticsInfos()
                        .Single(x => x.ResolveInfo?.Registration.ImplementationType == typeof(NoDependencyServiceA));
                    Assert.That(childInfo.ResolveInfo.Instances, Has.Count.EqualTo(1));
                    Assert.That(childInfo.ResolveInfo.Instances[0], Is.SameAs(instance));
                    Assert.That(childInfo.ResolveInfo.RefCount, Is.EqualTo(2));
                }

                // LifetimeScope removes its collector when disposed.
                DiagnositcsContext.RemoveCollector("Child");

                // The root keeps only its own instance, so the child's instance can be garbage collected.
                var rootInfo = DiagnositcsContext.GetDiagnosticsInfos()
                    .First(x => x.ScopeName == "Root");
                Assert.That(rootInfo.ResolveInfo.Instances, Has.Count.EqualTo(1));
            }
        }

        [TestCase(Lifetime.Transient)]
        [TestCase(Lifetime.Singleton)]
        public void NonScopedRegistrationsAreNotTrackedByChildScope(Lifetime lifetime)
        {
            var builder = new ContainerBuilder
            {
                Diagnostics = DiagnositcsContext.GetCollector("Root")
            };
            builder.Register<NoDependencyServiceA>(lifetime);

            using (var root = builder.Build())
            {
                root.Resolve<NoDependencyServiceA>();
                using (var child = root.CreateScope(childBuilder =>
                           childBuilder.Diagnostics = DiagnositcsContext.GetCollector("Child")))
                {
                    child.Resolve<NoDependencyServiceA>();

                    // Only Scoped registrations get a local entry in the child's collector.
                    Assert.That(DiagnositcsContext.GetCollector("Child").GetDiagnosticsInfos(), Is.Empty);
                }
            }
        }
    }
}
