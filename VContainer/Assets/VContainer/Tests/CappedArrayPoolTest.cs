using NUnit.Framework;
using VContainer.Internal;

namespace VContainer.Tests
{
    [TestFixture]
    public class CappedArrayPoolTest
    {
        [Test]
        public void RentWithExpandBucket()
        {
            var cappedArrayPool = new CappedArrayPool<object>(8);
            Assert.DoesNotThrow(() =>
            {
                for (var i = 1; i < CappedArrayPool<object>.InitialBucketSize * 2; i++)
                {
                    cappedArrayPool.Rent(1);
                }
            });
        }
    }
}