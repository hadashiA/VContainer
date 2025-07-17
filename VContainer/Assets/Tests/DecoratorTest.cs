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
        public readonly IDecoratedType Inner;

        public Decorator1(IDecoratedType inner)
        {
            Inner = inner;
        }
    }

    class Decorator2 : IDecoratedType
    {
        public readonly IDecoratedType Inner;

        public Decorator2(IDecoratedType inner)
        {
            Inner = inner;
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
            
            var inner = ((Decorator1)instance).Inner;
            Assert.That(inner, Is.TypeOf<DecoratedType>());
        }

        [Test]
        public void Decorate_Nested()
        {
            var builder = new ContainerBuilder();
            builder.Register<IDecoratedType, DecoratedType>(Lifetime.Singleton);
            builder.RegisterDecorator<IDecoratedType, Decorator1>();
            builder.RegisterDecorator<IDecoratedType, Decorator2>();
            
            var container = builder.Build();
            var instance = container.Resolve<IDecoratedType>();
            Assert.That(instance, Is.TypeOf<Decorator2>());
            
            var inner2 = ((Decorator2)instance).Inner;
            Assert.That(inner2, Is.TypeOf<Decorator1>());
            
            var inner1 = ((Decorator1)inner2).Inner;
            Assert.That(inner1, Is.TypeOf<DecoratedType>());
        }
    }
}
