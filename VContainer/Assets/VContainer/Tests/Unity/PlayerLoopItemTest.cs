using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using VContainer.Unity;
using Is = NUnit.Framework.Is;

namespace VContainer.Tests.Unity
{
    public class PlayerLoopItemTest
    {
        internal class TickableLoopItemTest
        {
            private class Ticker : ITickable
            {
                public void Tick() { }
            }

            [Test]
            public void MoveNextWithoutAllocation()
            {
                var list = new List<ITickable> { new Ticker(), new Ticker() };
                var exceptionHandler = new EntryPointExceptionHandler(exception => { });
                var tickableLoopItem = new TickableLoopItem(list, exceptionHandler);
                
                Assert.That(() =>
                {
                    tickableLoopItem.MoveNext();
                }, Is.Not.AllocatingGCMemory());
            }
        }
    }
}
