using System.Collections;
using NUnit.Framework;
using UnityEngine;
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

            var entryPoint = lifetimeScope.Container.Resolve<SampleEntryPoint>();
            Assert.That(entryPoint, Is.InstanceOf<SampleEntryPoint>());
            Assert.That(entryPoint.InitializeCalled, Is.EqualTo(1));
            Assert.That(entryPoint.PostInitializeCalled, Is.EqualTo(1));
            Assert.That(entryPoint.StartCalled, Is.EqualTo(1));
            Assert.That(entryPoint.PostStartCalled, Is.EqualTo(1));

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.That(entryPoint.FixedTickCalls, Is.GreaterThan(0));
            Assert.That(entryPoint.PostFixedTickCalls, Is.GreaterThan(0));

            var fixedTickCalls = entryPoint.FixedTickCalls;
            var postFixedTickCalls = entryPoint.PostFixedTickCalls;
            var tickCalls = entryPoint.TickCalls;
            var postTickCalls = entryPoint.PostTickCalls;
            var lateTickCalls = entryPoint.LateTickCalls;
            var postLateTickCalls = entryPoint.PostLateTickCalls;

            var disposable = lifetimeScope.Container.Resolve<DisposableServiceA>();
            lifetimeScope.Dispose();

            yield return null;
            yield return new WaitForFixedUpdate();

            Assert.That(fixedTickCalls, Is.EqualTo(fixedTickCalls));
            Assert.That(postFixedTickCalls, Is.EqualTo(postFixedTickCalls));
            Assert.That(tickCalls, Is.EqualTo(tickCalls));
            Assert.That(postTickCalls, Is.EqualTo(postTickCalls));
            Assert.That(lateTickCalls, Is.EqualTo(lateTickCalls));
            Assert.That(postLateTickCalls, Is.EqualTo(postLateTickCalls));
            Assert.That(disposable.Disposed, Is.True);
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

        [UnityTest]
        public IEnumerator FromParent_LifetimeScoped()
        {
            var parentScope = LifetimeScope.Create(builder =>
            {
                builder.RegisterEntryPoint<SampleEntryPoint>(Lifetime.Scoped).AsSelf();
            });
            var childScope = parentScope.CreateChild();

            yield return null;
            yield return null;

            var parentEntryPoint = parentScope.Container.Resolve<SampleEntryPoint>();
            var childEntryPoint = childScope.Container.Resolve<SampleEntryPoint>();
            Assert.That(parentEntryPoint.InitializeCalled, Is.EqualTo(1));
            Assert.That(parentEntryPoint.StartCalled, Is.EqualTo(1));
            Assert.That(childEntryPoint.InitializeCalled, Is.EqualTo(1));
            Assert.That(childEntryPoint.StartCalled, Is.EqualTo(1));
        }
    }
}