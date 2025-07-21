using System;

namespace VContainer.Benchmark.Fixtures
{
    public interface ISubObjectTwo
    {
    }

    public class SubObjectTwo : ISubObjectTwo
    {
        public SubObjectTwo(ISecondService secondService)
        {
            if (secondService == null)
            {
                throw new ArgumentNullException(nameof(secondService));
            }
        }
    }
}
