using System;

namespace VContainer.Benchmark.Fixtures
{
    public interface ISubObjectA
    {
        void Verify(string containerName);
    }

    public class SubObjectA : ISubObjectA
    {
        [Zenject.Inject]
        [VContainer.Inject]
        public IServiceA ServiceA { get; set; }

        public void Verify(string containerName)
        {
            if (this.ServiceA == null)
            {
                throw new Exception("ServiceA was null for SubObjectC for container " + containerName);
            }
        }
    }
}