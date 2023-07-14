using NUnit.Framework;
using VContainer.Internal;

namespace VContainer.Tests
{
    [TestFixture]
    public class CappedArrayPoolTest
    {
        [Test]
        public void Return_ClearElements()
        {
            var pool = new CappedArrayPool<object>(8);
            var a = pool.Rent(4);
            a[0] = new object();
            a[3] = new object();
            pool.Return(a);
            Assert.That(a[0], Is.Null);
            Assert.That(a[3], Is.Null);
        }

        [Test]
        public void Rent_ExpandBucket()
        {
            var pool = new CappedArrayPool<object>(8);
            Assert.DoesNotThrow(() =>
            {
                for (var i = 1; i < CappedArrayPool<object>.InitialBucketSize * 2; i++)
                {
                    pool.Rent(1);
                }
            });
        }
    }
}