using System;

namespace VContainer.Benchmark.Fixtures
{
    public interface ISubObjectC
    {
        void Verify(string containerName);
    }

    public class SubObjectC : ISubObjectC
    {
        public IServiceC ServiceC { get; set; }

        public void Verify(string containerName)
        {
            if (this.ServiceC == null)
            {
                throw new Exception("ServiceC was null for SubObjectC for container " + containerName);
            }
        }
    }
}