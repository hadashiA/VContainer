using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    public class UniTaskTest
    {
        [UnityTest]
        public IEnumerator AsyncStartup() => UniTask.ToCoroutine(async () =>
        {
            var parentLifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<SampleAsyncEntryPoint>(Lifetime.Scoped).AsSelf();
            });

            var entryPoint = parentLifetimeScope.Container.Resolve<SampleAsyncEntryPoint>();

            Assert.That(entryPoint.Started, Is.False);
            await UniTask.Yield();
            Assert.That(entryPoint.Started, Is.True);
        });
    }
}