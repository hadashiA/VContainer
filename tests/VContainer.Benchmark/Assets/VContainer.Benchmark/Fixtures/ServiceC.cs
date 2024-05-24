namespace VContainer.Benchmark.Fixtures
{
    public interface IServiceC
    {
    }

    public class ServiceC : IServiceC
    {
        [VContainer.Inject]
        [Zenject.Inject]
        public ServiceC()
        {
        }
    }
}