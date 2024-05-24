using System;

namespace VContainer.Benchmark.Fixtures
{
    public interface ISubObjectThree
    {
    }

    public class SubObjectThree : ISubObjectThree
    {
        public SubObjectThree(IThirdService thirdService)
        {
            if (thirdService == null)
            {
                throw new ArgumentNullException(nameof(thirdService));
            }
        }
    }
}
    