using System;

namespace VContainer.Benchmark.Fixtures
{
    public interface ISubObjectOne
    {
    }

    public class SubObjectOne : ISubObjectOne
    {
        public SubObjectOne(IFirstService firstService)
        {
            if (firstService == null)
            {
                throw new ArgumentNullException(nameof(firstService));
            }
        }
    }
}