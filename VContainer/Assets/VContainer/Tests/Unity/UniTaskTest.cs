using System;
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
            // var lifetimeScope = LifetimeScope.Create(builder =>
            // {
            //     builder.RegisterEntryPoint<SampleAsyncEntryPoint>(Lifetime.Scoped)
            //         .AsSelf();
            // });

            // var entryPoint = lifetimeScope.Container.Resolve<SampleAsyncEntryPoint>();

            UnityEngine.Debug.Log("000000");
            // Assert.That(entryPoint.Started, Is.False);
            // yield return null;
            await UniTask.Yield();
            UnityEngine.Debug.Log("yielded yielded yielded");
            // Assert.That(entryPoint.Started, Is.True);
        });

        [UnityTest]
        public IEnumerator AsyncStartupCancellable() => UniTask.ToCoroutine(async () =>
        {
            var lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<SampleAsyncStartableCancellable>(Lifetime.Scoped)
                    .AsSelf();
            });

            var entryPoint = lifetimeScope.Container.Resolve<SampleAsyncStartableCancellable>();

            Assert.That(entryPoint.Started, Is.False);
            lifetimeScope.Dispose();

            await UniTask.Yield();
            await UniTask.Yield();
            Assert.That(entryPoint.Started, Is.False);
            Assert.That(entryPoint.Cancelled, Is.False);
        });

        [UnityTest]
        public IEnumerator AsyncStartupExceptionHandler() => UniTask.ToCoroutine(async () =>
        {
            Exception caught = null;

            var lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<SampleAsyncStartableThrowable>(Lifetime.Scoped)
                    .AsSelf();

                builder.RegisterStartupExceptionHandler(ex => caught = ex);
            });

            var entryPoint = lifetimeScope.Container.Resolve<SampleAsyncStartableThrowable>();
            Assert.That(entryPoint, Is.InstanceOf<SampleAsyncStartableThrowable>());
            await UniTask.Yield();
            await UniTask.Yield();
            Assert.That(caught, Is.InstanceOf<InvalidOperationException>());
        });
    }
}
