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

            builder.RegisterFactory<Foo>(c0 =>
            {
                var dependency = c0.Resolve<I2>();
                return () => new Foo { Service2 = dependency };
            }, Lifetime.Transient);

            builder.RegisterFactory<int, Foo>(c1 =>
            {
                var dependency = c1.Resolve<I2>();
                return param1 => new Foo
                {
                    Param1 = param1 ,
                    Service2 = dependency
                };
            }, Lifetime.Transient);

            builder.RegisterFactory<int, int, Foo>(c2 =>
            {
                var dependency = c2.Resolve<I2>();
                return (param1, param2) => new Foo
                {
                    Param1 = param1,
                    Param2 = param2,
                    Service2 = dependency
                };
            }, Lifetime.Transient);

            builder.RegisterFactory<int, int, int, Foo>(c3 =>
            {
                var dependency = c3.Resolve<I2>();
                return (param1, param2, param3) => new Foo
                {
                    Param1 = param1,
                    Param2 = param2,
                    Param3 = param3,
                    Service2 = dependency
                };
            }, Lifetime.Transient);

            builder.RegisterFactory<int, int, int, int, Foo>(c4 =>
            {
                var dependency = c4.Resolve<I2>();
                return (param1, param2, param3, param4) => new Foo
                {
                    Param1 = param1,
                    Param2 = param2,
                    Param3 = param3,
                    Param4 = param4,
                    Service2 = dependency
                };
            }, Lifetime.Transient);

            var container = builder.Build();

            var func0A = container.Resolve<Func<Foo>>();
            var func0B = container.Resolve<Func<Foo>>();
            var foo0A = func0A();
            var foo0B = func0B();

            Assert.That(func0A, Is.TypeOf<Func<Foo>>());
            Assert.That(foo0A, Is.TypeOf<Foo>());
            Assert.That(foo0A.Service2, Is.InstanceOf<I2>());
            Assert.That(foo0A.Service2, Is.Not.EqualTo(foo0B.Service2));

            var func1A = container.Resolve<Func<int, Foo>>();
            var func1B = container.Resolve<Func<int, Foo>>();
            var foo1A = func1A(100);
            var foo1B = func1B(100);

            Assert.That(func1A, Is.TypeOf<Func<int, Foo>>());
            Assert.That(foo1A, Is.TypeOf<Foo>());
            Assert.That(foo1A.Service2, Is.InstanceOf<I2>());
            Assert.That(foo1A.Service2, Is.Not.EqualTo(foo1B.Service2));
            Assert.That(foo1A.Param1, Is.EqualTo(100));

            var func2A = container.Resolve<Func<int, int, Foo>>();
            var func2B = container.Resolve<Func<int, int, Foo>>();
            var foo2A = func2A(100, 200);
            var foo2B = func2B(100, 200);

            Assert.That(func2A, Is.TypeOf<Func<int, int, Foo>>());
            Assert.That(foo2A, Is.TypeOf<Foo>());
            Assert.That(foo2A.Service2, Is.InstanceOf<I2>());
            Assert.That(foo2A.Service2, Is.Not.EqualTo(foo2B.Service2));
            Assert.That(foo2A.Param1, Is.EqualTo(100));
            Assert.That(foo2A.Param2, Is.EqualTo(200));

            var func3A = container.Resolve<Func<int, int, int, Foo>>();
            var func3B = container.Resolve<Func<int, int, int, Foo>>();
            var foo3A = func3A(100, 200, 300);
            var foo3B = func3B(100, 200, 300);

            Assert.That(func3A, Is.TypeOf<Func<int, int, int, Foo>>());
            Assert.That(foo3A, Is.TypeOf<Foo>());
            Assert.That(foo3A.Service2, Is.InstanceOf<I2>());
            Assert.That(foo3A.Service2, Is.Not.EqualTo(foo3B.Service2));
            Assert.That(foo3A.Param1, Is.EqualTo(100));
            Assert.That(foo3A.Param2, Is.EqualTo(200));
            Assert.That(foo3A.Param3, Is.EqualTo(300));

            var func4A = container.Resolve<Func<int, int, int, int, Foo>>();
            var func4B = container.Resolve<Func<int, int, int, int, Foo>>();
            var foo4A = func4A(100, 200, 300, 400);
            var foo4B = func4B(100, 200, 300, 400);

            Assert.That(func4A, Is.TypeOf<Func<int, int, int, int, Foo>>());
            Assert.That(foo4A, Is.TypeOf<Foo>());
            Assert.That(foo4A.Service2, Is.InstanceOf<I2>());
            Assert.That(foo4A.Service2, Is.Not.EqualTo(foo4B.Service2));
            Assert.That(foo4A.Param1, Is.EqualTo(100));
            Assert.That(foo4A.Param2, Is.EqualTo(200));
            Assert.That(foo4A.Param3, Is.EqualTo(300));
            Assert.That(foo4A.Param4, Is.EqualTo(400));
        }

        [Test]
        public void RegisterFactoryWithContainerScoped()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Transient);

            builder.RegisterFactory<Foo>(c0 =>
            {
                var dependency = c0.Resolve<I2>();
                return () => new Foo { Service2 = dependency };
            }, Lifetime.Scoped);

            builder.RegisterFactory<int, Foo>(c1 =>
            {
                var dependency = c1.Resolve<I2>();
                return param1 => new Foo { Service2 = dependency, Param1 = param1 };
            }, Lifetime.Scoped);

            builder.RegisterFactory<int, int, Foo>(c2 =>
            {
                var dependency = c2.Resolve<I2>();
                return (param1, param2) => new Foo
                {
                    Service2 = dependency,
                    Param1 = param1,
                    Param2 = param2
                };
            }, Lifetime.Scoped);

            builder.RegisterFactory<int, int, Foo>(c3 =>
            {
                var dependency = c3.Resolve<I2>();
                return (param1, param2) => new Foo
                {
                    Service2 = dependency,
                    Param1 = param1,
                    Param2 = param2
                };
            }, Lifetime.Scoped);

            builder.RegisterFactory<int, int, int, Foo>(c4 =>
            {
                var dependency = c4.Resolve<I2>();
                return (param1, param2, param3) => new Foo
                {
                    Service2 = dependency,
                    Param1 = param1,
                    Param2 = param2,
                    Param3 = param3
                };
            }, Lifetime.Scoped);

            builder.RegisterFactory<int, int, int, int, Foo>(c5 =>
            {
                var dependency = c5.Resolve<I2>();
                return (param1, param2, param3, param4) => new Foo
                {
                    Service2 = dependency,
                    Param1 = param1,
                    Param2 = param2,
                    Param3 = param3,
                    Param4 = param4
                };
            }, Lifetime.Scoped);

            var container = builder.Build();

            var func0A = container.Resolve<Func<Foo>>();
            var func0B = container.Resolve<Func<Foo>>();
            var foo0A = func0A();
            var foo0B = func0B();

            Assert.That(func0A, Is.TypeOf<Func<Foo>>());
            Assert.That(foo0A, Is.TypeOf<Foo>());
            Assert.That(foo0A.Service2, Is.InstanceOf<I2>());
            Assert.That(foo0A.Service2, Is.EqualTo(foo0B.Service2));

            var func1A = container.Resolve<Func<int, Foo>>();
            var func1B = container.Resolve<Func<int, Foo>>();
            var foo1A = func1A(100);
            var foo1B = func1B(100);

            Assert.That(func1A, Is.TypeOf<Func<int, Foo>>());
            Assert.That(foo1A, Is.TypeOf<Foo>());
            Assert.That(foo1A.Service2, Is.InstanceOf<I2>());
            Assert.That(foo1A.Service2, Is.EqualTo(foo1B.Service2));
            Assert.That(foo1A.Param1, Is.EqualTo(100));

            var func2A = container.Resolve<Func<int, int, Foo>>();
            var func2B = container.Resolve<Func<int, int, Foo>>();
            var foo2A = func2A(100, 200);
            var foo2B = func2B(100, 200);

            Assert.That(func2A, Is.TypeOf<Func<int, int, Foo>>());
            Assert.That(foo2A, Is.TypeOf<Foo>());
            Assert.That(foo2A.Service2, Is.InstanceOf<I2>());
            Assert.That(foo2A.Service2, Is.EqualTo(foo2B.Service2));
            Assert.That(foo2A.Param1, Is.EqualTo(100));
            Assert.That(foo2A.Param2, Is.EqualTo(200));

            var func3A = container.Resolve<Func<int, int, int, Foo>>();
            var func3B = container.Resolve<Func<int, int, int, Foo>>();
            var foo3A = func3A(100, 200, 300);
            var foo3B = func3B(100, 200, 300);

            Assert.That(func3A, Is.TypeOf<Func<int, int, int, Foo>>());
            Assert.That(foo3A, Is.TypeOf<Foo>());
            Assert.That(foo3A.Service2, Is.InstanceOf<I2>());
            Assert.That(foo3A.Service2, Is.EqualTo(foo3B.Service2));
            Assert.That(foo3A.Param1, Is.EqualTo(100));
            Assert.That(foo3A.Param2, Is.EqualTo(200));
            Assert.That(foo3A.Param3, Is.EqualTo(300));

            var func4A = container.Resolve<Func<int, int, int, int, Foo>>();
            var func4B = container.Resolve<Func<int, int, int, int, Foo>>();
            var foo4A = func4A(100, 200, 300, 400);
            var foo4B = func4B(100, 200, 300, 400);

            Assert.That(func4A, Is.TypeOf<Func<int, int, int, int, Foo>>());
            Assert.That(foo4A, Is.TypeOf<Foo>());
            Assert.That(foo4A.Service2, Is.InstanceOf<I2>());
            Assert.That(foo4A.Service2, Is.EqualTo(foo4B.Service2));
            Assert.That(foo4A.Param1, Is.EqualTo(100));
            Assert.That(foo4A.Param2, Is.EqualTo(200));
            Assert.That(foo4A.Param3, Is.EqualTo(300));
            Assert.That(foo4A.Param4, Is.EqualTo(400));

        }
    }
}