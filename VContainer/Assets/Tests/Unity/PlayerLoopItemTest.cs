using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using VContainer.Unity;
using Is = NUnit.Framework.Is;

namespace VContainer.Tests.Unity
{
    public class PlayerLoopItemTest
    {
        class Ticker : ITickable, IPostTickable, IPostLateTickable, IFixedTickable, IPostFixedTickable
        {
            public void Tick() { }
            public void FixedTick() { }
            public void PostTick() { }
            public void PostLateTick() { }
            public void PostFixedTick() { }
        }

        class TestStartable : IStartable
        {
            public int Executed { get; private set; }

            public void Start()
            {
                Executed++;
            }
        }

        class ThrowStartable : IStartable
        {
            public int Executed { get; private set; }

            public void Start()
            {
                Executed++;
                throw new InvalidOperationException("OOPS");
            }
        }

        [Test]
        public void Start_Throw()
        {
            var item = new TestStartable();
            var thrownItem = new ThrowStartable();
            var exceptionHandler = new EntryPointExceptionHandler(ex => { });
            var loopItem = new StartableLoopItem(new IStartable[] { thrownItem, item }, exceptionHandler);

            Assert.That(loopItem.MoveNext(), Is.False);
        }

        [Test]
        public void Tick_MoveNextWithoutAllocation()
        {
            var list = new List<ITickable> { new Ticker(), new Ticker() };
            var exceptionHandler = new EntryPointExceptionHandler(exception => { });
            var tickableLoopItem = new TickableLoopItem(list, exceptionHandler);

            Assert.That(() =>
            {
                tickableLoopItem.MoveNext();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void PostTick_MoveNextWithoutAllocation()
        {
            var list = new List<IPostTickable> { new Ticker(), new Ticker() };
            var exceptionHandler = new EntryPointExceptionHandler(exception => { });
            var tickableLoopItem = new PostTickableLoopItem(list, exceptionHandler);

            Assert.That(() =>
            {
                tickableLoopItem.MoveNext();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void LateTick_MoveNextWithoutAllocation()
        {
            var list = new List<IPostLateTickable> { new Ticker(), new Ticker() };
            var exceptionHandler = new EntryPointExceptionHandler(exception => { });
            var tickableLoopItem = new PostLateTickableLoopItem(list, exceptionHandler);

            Assert.That(() =>
            {
                tickableLoopItem.MoveNext();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void FixedTick_MoveNextWithoutAllocation()
        {
            var list = new List<IFixedTickable> { new Ticker(), new Ticker() };
            var exceptionHandler = new EntryPointExceptionHandler(exception => { });
            var tickableLoopItem = new FixedTickableLoopItem(list, exceptionHandler);

            Assert.That(() =>
            {
                tickableLoopItem.MoveNext();
            }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void PostFixedTick_MoveNextWithoutAllocation()
        {
            var list = new List<IPostFixedTickable> { new Ticker(), new Ticker() };
            var exceptionHandler = new EntryPointExceptionHandler(exception => { });
            var tickableLoopItem = new PostFixedTickableLoopItem(list, exceptionHandler);

            Assert.That(() =>
            {
                tickableLoopItem.MoveNext();
            }, Is.Not.AllocatingGCMemory());
        }
    }
}