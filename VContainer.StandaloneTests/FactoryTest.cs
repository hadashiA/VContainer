using System;
using NUnit.Framework;

namespace VContainer.Tests
{
    class Foo
    {
        public int Param1 { get; set; }
        public int Param2 { get; set; }
        public int Param3 { get; set; }
        public int Param4 { get; set; }
        public I2 Service2 { get; set; }
        public I3 Service3 { get; set; }
    }

    [TestFixture]
    public class FactoryTest
    {
        [Test]
        public void RegisterFactoryWithParams()
        {
            var builder = new ContainerBuilder();

            builder.RegisterFactory(() => new Foo());
            builder.RegisterFactory<int, Foo>(param1 => new Foo { Param1 = param1});
            builder.RegisterFactory<int, int, Foo>((param1, param2) => new Foo
            {
                Param1 = param1,
                Param2 = param2
            });
            builder.RegisterFactory<int, int, int, Foo>((param1, param2, param3) => new Foo
            {
                Param1 = param1,
                Param2 = param2,
                Param3 = param3,
            });
            builder.RegisterFactory<int, int, int, int, Foo>((param1, param2, param3, param4) => new Foo
            {
                Param1 = param1,
                Param2 = param2,
                Param3 = param3,
                Param4 = param4,
            });

            var container = builder.Build();

            var func0 = container.Resolve<Func<Foo>>();
            Assert.That(func0(), Is.TypeOf<Foo>());

            var func1 = container.Resolve<Func<int, Foo>>();
            var foo1 = func1(100);
            Assert.That(func1, Is.TypeOf<Func<int, Foo>>());
            Assert.That(foo1.Param1, Is.EqualTo(100));

            var func2 = container.Resolve<Func<int, int, Foo>>();
            var foo2 = func2(100, 200);
            Assert.That(func2, Is.TypeOf<Func<int, int, Foo>>());
            Assert.That(foo2.Param1, Is.EqualTo(100));
            Assert.That(foo2.Param2, Is.EqualTo(200));

            var func3 = container.Resolve<Func<int, int, int, Foo>>();
            var foo3 = func3(100, 200, 300);
            Assert.That(func3, Is.TypeOf<Func<int, int, int, Foo>>());
            Assert.That(foo3.Param1, Is.EqualTo(100));
            Assert.That(foo3.Param2, Is.EqualTo(200));
            Assert.That(foo3.Param3, Is.EqualTo(300));

            var func4 = container.Resolve<Func<int, int, int, int, Foo>>();
            var foo4 = func4(100, 200, 300, 400);
            Assert.That(func4, Is.TypeOf<Func<int, int, int, int, Foo>>());
            Assert.That(foo4.Param1, Is.EqualTo(100));
            Assert.That(foo4.Param2, Is.EqualTo(200));
            Assert.That(foo4.Param3, Is.EqualTo(300));
            Assert.That(foo4.Param4, Is.EqualTo(400));
        }

        [Test]
        public void RegisterFactoryWithContainerTransient()
        {
            var builder = new ContainerBuilder();

            builder.Register<I2, NoDependencyServiceA>(Lifetime.Transient);

            builder.RegisterFactory<Foo>(container =>
            {
                return () => new Foo { Service2 = container.Resolve<I2>() };
            }, Lifetime.Transient);

            var container = builder.Build();
            var func0 = container.Resolve<Func<Foo>>();
            var foo0 = func0();
            Assert.That(func0, Is.TypeOf<Func<Foo>>());
            Assert.That(foo0, Is.TypeOf<Foo>());
            Assert.That(foo0.Service2, Is.InstanceOf<>());
        }
    }
}