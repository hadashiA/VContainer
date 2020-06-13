using System;
using NUnit.Framework;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    class OneshotLoopItem : IPlayerLoopItem
    {
        public int Called { get; private set; }

        public bool MoveNext()
        {
            Called++;
            return false;
        }
    }

    class DisposableLoopItem : IPlayerLoopItem, IDisposable
    {
        public int Called { get; private set;  }

        bool disposed;

        public bool MoveNext()
        {
            if (disposed) return false;
            Called++;
            return !disposed;
        }

        public void Dispose() => disposed = true;
    }

    class NestedLoopItem : IPlayerLoopItem
    {
        public int Called { get; private set; }
        public OneshotLoopItem ChildLoopItem { get; } = new OneshotLoopItem();

        readonly PlayerLoopRunner runner;

        public NestedLoopItem(PlayerLoopRunner runner)
        {
            this.runner = runner;
        }

        public bool MoveNext()
        {
            if (Called == 0)
            {
                runner.Dispatch(ChildLoopItem);
            }
            Called++;
            return true;
        }
    }

    [TestFixture]
    public class PlayerLoopRunnerTest
    {
        [Test]
        public void Run()
        {
            var runner = new PlayerLoopRunner();

            var oneshot = new OneshotLoopItem();
            var disposable = new DisposableLoopItem();
            var nested = new NestedLoopItem(runner);
            runner.Dispatch(oneshot);
            runner.Dispatch(disposable);
            runner.Dispatch(nested);
            runner.Run();
            runner.Run();
            disposable.Dispose();
            runner.Run();

            Assert.That(oneshot.Called, Is.EqualTo(1));
            Assert.That(disposable.Called, Is.EqualTo(2));
            Assert.That(nested.Called, Is.EqualTo(3));
            Assert.That(nested.ChildLoopItem.Called, Is.EqualTo(1));
        }
    }
}