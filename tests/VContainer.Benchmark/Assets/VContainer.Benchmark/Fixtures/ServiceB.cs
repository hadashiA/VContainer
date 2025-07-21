namespace VContainer.Benchmark.Fixtures
{
    public interface IServiceB
    {
    }

    public class ServiceB : IServiceB
    {
        [VContainer.Inject]
        [Zenject.Inject]
        public ServiceB()
        {
        }
    }
}
