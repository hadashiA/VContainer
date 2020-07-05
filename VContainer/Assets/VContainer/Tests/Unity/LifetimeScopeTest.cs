using UnityEngine;
using NUnit.Framework;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    public class LifetimeScopeTest
    {
        [Test]
        public void PushParent()
        {
            var parent = new GameObject("LifetimeScope").AddComponent<LifetimeScope>();

            using (LifetimeScope.PushParent(parent))
            {
                var child = new GameObject("LifetimeScope Child 1").AddComponent<LifetimeScope>();
                Assert.That(child.Parent, Is.EqualTo(parent));
            }

            var child2 = new GameObject("LifetimeScope Child 2").AddComponent<LifetimeScope>();
            Assert.That(child2.Parent, Is.Null);
        }
    }
}