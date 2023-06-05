#if VCONTAINER_UNITASK_INTEGRATION
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace VContainer.Tests
{
    public class AsyncRegisterTest
    {
        [Test]
        public async Task ResolveTypeFromAsyncMethod()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAsync(CreateAsync, Lifetime.Singleton);

            var container = builder.Build();

            var foo = await container.Resolve<UniTask<Foo>>();
            Assert.That(foo, Is.TypeOf<Foo>());

            UniTask<Foo> CreateAsync(IObjectResolver resolver) => UniTask.FromResult(new Foo());
        }
        
        [Test]
        public async Task ResolveFromResolveAsync()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Transient);

            builder.RegisterAsync(Create, Lifetime.Scoped);

            var container = builder.Build();
            var resolved = await container.ResolveAsync<ServiceA>();
            Assert.That(resolved, Is.InstanceOf<ServiceA>());
            Assert.That(resolved.Service2, Is.InstanceOf<NoDependencyServiceA>());

            UniTask<ServiceA> Create(IObjectResolver resolver)
            {
                return UniTask.FromResult(new ServiceA(resolver.Resolve<I2>()));
            }
        }
    }
}
#endif