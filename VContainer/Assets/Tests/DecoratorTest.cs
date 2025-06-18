using NUnit.Framework;
using VContainer.Runtime;

namespace VContainer.Tests
{
    interface IDecoratedType
    {
    }

    class DecoratedType : IDecoratedType
    {
    }

    class Decorator1 : IDecoratedType
    {
        readonly IDecoratedType inner;

        public Decorator1(IDecoratedType inner)
        {
            this.inner = inner;
        }
    }

    class Decorator2 : IDecoratedType
    {
        readonly IDecoratedType inner;

        public Decorator2(IDecoratedType inner)
        {
            this.inner = inner;
        }
    }

    [TestFixture]
    public class DecoratorTest
    {
        [Test]
        public void Decorate()
        {
            var builder = new ContainerBuilder();
            builder.Register<IDecoratedType, DecoratedType>(Lifetime.Singleton);
            builder.RegisterDecorator<IDecoratedType, Decorator1>();

            var container = builder.Build();
            var instance = container.Resolve<IDecoratedType>();
            Assert.That(instance, Is.TypeOf<Decorator1>());
        }
    }
}
