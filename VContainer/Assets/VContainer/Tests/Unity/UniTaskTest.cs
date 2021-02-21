#if VCONTAINER_UNITASK_INTEGRATION
using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    public class SampleAsyncStartable : IAsyncStartable
    {
        public bool Started;

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            await UniTask.Yield();
            Started = true;
        }
    }

    public class SampleAsyncStartableCancellable : IAsyncStartable
    {
        public bool Started;
        public bool Cancelled;

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            using (cancellation.Register(() => Cancelled = true))
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellation);
                Started = true;
            }
        }
    }

    public class SampleAsyncStartableThrowable : IAsyncStartable
    {
        public async UniTask StartAsync(CancellationToken cancellation)
        {
            await UniTask.Yield();
            throw new InvalidOperationException("Something went happen");
        }
    }

    public class UniTaskTest
    {
        [UnityTest]
        public IEnumerator AsyncStartup() => UniTask.ToCoroutine(async () =>
        {
            var lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<SampleAsyncStartable>(Lifetime.Scoped)
                    .AsSelf();
            });

            var entryPoint = lifetimeScope.Container.Resolve<SampleAsyncStartable>();

            Assert.That(entryPoint.Started, Is.False);
            await UniTask.Yield();
            await UniTask.Yield();
            Assert.That(entryPoint.Started, Is.True);
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

                builder.RegisterEntryPointExceptionHandler(ex => caught = ex);
            });

            var entryPoint = lifetimeScope.Container.Resolve<SampleAsyncStartableThrowable>();
            Assert.That(entryPoint, Is.InstanceOf<SampleAsyncStartableThrowable>());
            await UniTask.Yield();
            await UniTask.Yield();
            Assert.That(caught, Is.InstanceOf<InvalidOperationException>());
        });
    }
}
#endif
