using VContainer.Unity.Extensions;

namespace VContainer.Tests.Unity
{
    public sealed class SampleSingletonLifetimeScope : SingletonLifetimeScope<SampleSingletonLifetimeScope>
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<DisposableServiceA>(Lifetime.Scoped);
            builder.Register<DisposableServiceB>(Lifetime.Singleton);
        }
    }
}