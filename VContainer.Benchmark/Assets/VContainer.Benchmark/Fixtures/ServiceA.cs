namespace VContainer.Benchmark.Fixtures
{
    public interface IServiceA
    {
    }

    public class ServiceA : IServiceA
    {
        [VContainer.Inject]
        [Zenject.Inject]
        public ServiceA()
        {
        }
    }
}