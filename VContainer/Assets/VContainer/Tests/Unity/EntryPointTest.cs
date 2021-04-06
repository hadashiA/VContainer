using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    public class EntryPointTest
    {
        [UnityTest]
        public IEnumerator Create()
        {
            var lifetimeScope = LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<SampleEntryPoint>().AsSelf();
                builder.Register<DisposableServiceA>(Lifetime.Scoped);
            });

            yield return null;
            yield return null;
            yield return null;

            var entryPoint = lifetimeScope.Container.Resolve<SampleEntryPoint>();
            Assert.That(entryPoint, Is.InstanceOf<SampleEntryPoint>());

            Assert.That(entryPoint.InitializeCalled, Is.EqualTo(1));
            Assert.That(entryPoint.PostInitializeCalled, Is.EqualTo(1));
            Assert.That(entryPoint.StartCalled, Is.EqualTo(1));
            Assert.That(entryPoint.PostStartCalled, Is.EqualTo(1));

            var fixedTickCalls = entryPoint.FixedTickCalls;
            var postFixedTickCalls = entryPoint.PostFixedTickCalls;
            var tickCalls = entryPoint.TickCalls;
            var postTickCalls = entryPoint.PostTickCalls;
            var lateTickCalls = entryPoint.LateTickCalls;
            var postLateTickCalls = entryPoint.PostLateTickCalls;

            var disposable = lifetimeScope.Container.Resolve<DisposableServiceA>();
            lifetimeScope.Dispose();

            Assert.That(fixedTickCalls, Is.GreaterThan(0));
            Assert.That(postFixedTickCalls, Is.GreaterThan(0));
            Assert.That(tickCalls, Is.GreaterThan(1));
            Assert.That(postTickCalls, Is.GreaterThan(1));
            Assert.That(lateTickCalls, Is.GreaterThan(1));
            Assert.That(postLateTickCalls, Is.GreaterThan(1));

            yield return null;
            yield return null;

            Assert.That(disposable.Disposed, Is.True);
            Assert.That(entryPoint.FixedTickCalls, Is.EqualTo(fixedTickCalls));
            Assert.That(entryPoint.PostFixedTickCalls, Is.EqualTo(postFixedTickCalls));
            Assert.That(entryPoint.TickCalls, Is.EqualTo(tickCalls));
            Assert.That(entryPoint.PostTickCalls, Is.EqualTo(postTickCalls));
            Assert.That(entryPoint.LateTickCalls, Is.EqualTo(lateTickCalls));
            Assert.That(entryPoint.PostLateTickCalls, Is.EqualTo(postLateTickCalls));
        }

        [UnityTest]
        public IEnumerator InitializeExceptionHandler()
        {
            var handled = 0;

            LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<InitializableThrowable>();
                builder.RegisterEntryPointExceptionHandler(ex => handled += 1);
            });

            yield return null;
            yield return null;

            Assert.That(handled, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PostInitializeExceptionHandler()
        {
            var handled = 0;

            LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<PostInitializableThrowable>();
                builder.RegisterEntryPointExceptionHandler(ex => { handled += 1; });
            });

            yield return null;
            yield return null;

            Assert.That(handled, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator StartableExceptionHandler()
        {
            var handled = 0;

            LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<StartableThrowable>();
                builder.RegisterEntryPointExceptionHandler(ex => { handled += 1; });
            });

            yield return null;
            yield return null;

            Assert.That(handled, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PostStartableExceptionHandler()
        {
            var handled = 0;

            LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<PostStartableThrowable>();
                builder.RegisterEntryPointExceptionHandler(ex => { handled += 1; });
            });

            yield return null;
            yield return null;

            Assert.That(handled, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TickableExceptionHandler()
        {
            var handled = 0;

            LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<TickableThrowable>();
                builder.RegisterEntryPointExceptionHandler(ex => { handled += 1; });
            });

            yield return null;
            yield return null;

            Assert.That(handled, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PostTickableExceptionHandler()
        {
            var handled = 0;

            LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<PostTickableThrowable>();
                builder.RegisterEntryPointExceptionHandler(ex => handled += 1);
            });

            yield return null;
            yield return null;

            Assert.That(handled, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator LateTickableExceptionHandler()
        {
            var handled = 0;

            LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<LateTickableThrowable>();
                builder.RegisterEntryPointExceptionHandler(ex => handled += 1);
            });

            yield return null;
            yield return null;

            Assert.That(handled, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PostLateTickableExceptionHandler()
        {
            var handled = 0;

            LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<PostLateTickableThrowable>();
                builder.RegisterEntryPointExceptionHandler(ex => handled += 1);
            });

            yield return null;
            yield return null;

            Assert.That(handled, Is.EqualTo(1));
        }
    }
}