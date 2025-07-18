using NUnit.Framework;
using VContainer.Runtime;

namespace VContainer.Tests
{
    [TestFixture]
    public class DecoratorTest
    {
        [Test]
        public void Decorate()
        {
            var builder = new ContainerBuilder();
            builder.Register<Mocks.IDecoratedType, Mocks.DecoratedType>(Lifetime.Singleton);
            builder.RegisterDecorator<Mocks.IDecoratedType, Mocks.Decorator1>();

            var container = builder.Build();
            var instance = container.Resolve<Mocks.IDecoratedType>();
            Assert.That(instance, Is.TypeOf<Mocks.Decorator1>());
            
            var inner = ((Mocks.Decorator1)instance).Inner;
            Assert.That(inner, Is.TypeOf<Mocks.DecoratedType>());
        }

        [Test]
        public void Decorate_Nested()
        {
            var builder = new ContainerBuilder();
            builder.Register<Mocks.IDecoratedType, Mocks.DecoratedType>(Lifetime.Singleton);
            builder.RegisterDecorator<Mocks.IDecoratedType, Mocks.Decorator1>();
            builder.RegisterDecorator<Mocks.IDecoratedType, Mocks.Decorator2>();
            
            var container = builder.Build();
            var instance = container.Resolve<Mocks.IDecoratedType>();
            Assert.That(instance, Is.TypeOf<Mocks.Decorator2>());
            
            var inner2 = ((Mocks.Decorator2)instance).Inner;
            Assert.That(inner2, Is.TypeOf<Mocks.Decorator1>());
            
            var inner1 = ((Mocks.Decorator1)inner2).Inner;
            Assert.That(inner1, Is.TypeOf<Mocks.DecoratedType>());
        }
        
        [Test]
        public void Decorate_Func()
        {
            var builder = new ContainerBuilder();
            builder.Register<Mocks.IDecoratedType, Mocks.DecoratedType>(Lifetime.Singleton);
            builder.RegisterDecorator<Mocks.IDecoratedType, Mocks.Decorator1>(
                (original, container) => new Mocks.Decorator1(original));

            var container = builder.Build();
            var instance = container.Resolve<Mocks.IDecoratedType>();
            Assert.That(instance, Is.TypeOf<Mocks.Decorator1>());
            
            var inner = ((Mocks.Decorator1)instance).Inner;
            Assert.That(inner, Is.TypeOf<Mocks.DecoratedType>());
        }
        
        [Test]
        public void Decorate_Func_Nested()
        {
            var builder = new ContainerBuilder();
            builder.Register<Mocks.IDecoratedType, Mocks.DecoratedType>(Lifetime.Singleton);
            builder.RegisterDecorator<Mocks.IDecoratedType, Mocks.Decorator1>(
                (original, container) => new Mocks.Decorator1(original));
            builder.RegisterDecorator<Mocks.IDecoratedType, Mocks.Decorator2>(
                (decorated, container) => new Mocks.Decorator2(decorated));
            
            var container = builder.Build();
            var instance = container.Resolve<Mocks.IDecoratedType>();
            Assert.That(instance, Is.TypeOf<Mocks.Decorator2>());
            
            var inner2 = ((Mocks.Decorator2)instance).Inner;
            Assert.That(inner2, Is.TypeOf<Mocks.Decorator1>());
            
            var inner1 = ((Mocks.Decorator1)inner2).Inner;
            Assert.That(inner1, Is.TypeOf<Mocks.DecoratedType>());
        }

        [Test]
        public void Decorate_Does_Not_Register_To_Non_Related_Interfaces_Of_Decorated_Type()
        {
            var builder = new ContainerBuilder();
            builder.Register<Mocks.DecoratedTypeWithNonRelatedInterface>(Lifetime.Singleton)
                .As<Mocks.IDecoratedType, Mocks.IOtherInterface>();
            
            builder.RegisterDecorator<Mocks.IDecoratedType, Mocks.Decorator1>();

            var container = builder.Build();
            var instance = container.Resolve<Mocks.IDecoratedType>();
            Assert.That(instance, Is.TypeOf<Mocks.Decorator1>());
            
            var otherInstance = container.Resolve<Mocks.IOtherInterface>();
            Assert.That(otherInstance, Is.TypeOf<Mocks.DecoratedTypeWithNonRelatedInterface>());
        }

        [Test]
        public void Decorate_Supports_Registration_To_Additional_Interfaces()
        {
            var builder = new ContainerBuilder();
            builder.Register<Mocks.IDecoratedType, Mocks.DecoratedType>(Lifetime.Singleton);
            builder.RegisterDecorator<Mocks.IDecoratedType, Mocks.DecoratorWithAdditionalInterface>()
                .As<Mocks.IOtherInterface>();

            var container = builder.Build();
            var instance = container.Resolve<Mocks.IDecoratedType>();
            Assert.That(instance, Is.TypeOf<Mocks.DecoratorWithAdditionalInterface>());
            
            var otherInstance = container.Resolve<Mocks.IOtherInterface>();
            Assert.That(otherInstance, Is.TypeOf<Mocks.DecoratorWithAdditionalInterface>());
        }

        [Test]
        public void Decorate_Automatically_Registers_To_All_Bound_Base_Interfaces()
        {
            var builder = new ContainerBuilder();
            builder.Register<Mocks.ExtendedDecoratedType>(Lifetime.Singleton)
                .As<Mocks.IDecoratedType, Mocks.IExtendedDecoratedType>();
            
            builder.RegisterDecorator<Mocks.IExtendedDecoratedType, Mocks.ExtendedDecorator>();

            var container = builder.Build();
            var instance = container.Resolve<Mocks.IExtendedDecoratedType>();
            Assert.That(instance, Is.TypeOf<Mocks.ExtendedDecorator>());
            
            var baseInstance = container.Resolve<Mocks.IDecoratedType>();
            Assert.That(baseInstance, Is.TypeOf<Mocks.ExtendedDecorator>());
        }

        [Test]
        public void Decorate_Parent_Registration()
        {
            var parentBuilder = new ContainerBuilder();
            parentBuilder.Register<Mocks.IDecoratedType, Mocks.DecoratedType>(Lifetime.Singleton);

            using var parentContainer = parentBuilder.Build();
            using var childContainer = parentContainer.CreateScope(builder =>
            {
                builder.RegisterDecorator<Mocks.IDecoratedType, Mocks.Decorator1>();
            });
            
            var instance = childContainer.Resolve<Mocks.IDecoratedType>();
            Assert.That(instance, Is.TypeOf<Mocks.Decorator1>());
            
            var inner = ((Mocks.Decorator1)instance).Inner;
            Assert.That(inner, Is.TypeOf<Mocks.DecoratedType>());
        }


        private static class Mocks
        {
            public interface IDecoratedType
            {
            }

            public class DecoratedType : IDecoratedType
            {
            }

            public class Decorator1 : IDecoratedType
            {
                public readonly IDecoratedType Inner;

                public Decorator1(IDecoratedType inner)
                {
                    Inner = inner;
                }
            }

            public class Decorator2 : IDecoratedType
            {
                public readonly IDecoratedType Inner;

                public Decorator2(IDecoratedType inner)
                {
                    Inner = inner;
                }
            }
            
            public interface IOtherInterface
            {
            }
            
            public class DecoratedTypeWithNonRelatedInterface : IDecoratedType, IOtherInterface
            {
            }

            public class DecoratorWithAdditionalInterface : IDecoratedType, IOtherInterface
            {
                public readonly IDecoratedType Inner;

                public DecoratorWithAdditionalInterface(IDecoratedType inner)
                {
                    Inner = inner;
                }
            }
            
            public interface IExtendedDecoratedType : IDecoratedType
            {
            }

            public class ExtendedDecoratedType : IExtendedDecoratedType
            {
            }

            public class ExtendedDecorator : IExtendedDecoratedType
            {
                public readonly IExtendedDecoratedType Inner;

                public ExtendedDecorator(IExtendedDecoratedType inner)
                {
                    Inner = inner;
                }
            }
        }
    }
}
