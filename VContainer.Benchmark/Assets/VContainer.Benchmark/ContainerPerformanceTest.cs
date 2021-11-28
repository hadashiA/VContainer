using NUnit.Framework;
using Unity.PerformanceTesting;
using VContainer;
using VContainer.Benchmark.Fixtures;

namespace Vcontainer.Benchmark
{
    public class ContainerPerformanceTest
    {
        const int N = 10_000;

        [Test]
        [Performance]
        public void ResolveSingleton()
        {
            var zenjectContainer = new Zenject.DiContainer();
            zenjectContainer.Bind<ISingleton1>().To<Singleton1>().AsSingle();
            zenjectContainer.Bind<ISingleton2>().To<Singleton2>().AsSingle();
            zenjectContainer.Bind<ISingleton3>().To<Singleton3>().AsSingle();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        zenjectContainer.Resolve<ISingleton1>();
                        zenjectContainer.Resolve<ISingleton2>();
                        zenjectContainer.Resolve<ISingleton3>();
                    }
                })
                .SampleGroup("Zenject")
                .GC()
                .Run();

            var reflexContainer = new Reflex.Container();
            reflexContainer.Bind<ISingleton1>().To<Singleton1>().AsSingletonLazy();
            reflexContainer.Bind<ISingleton2>().To<Singleton2>().AsSingletonLazy();
            reflexContainer.Bind<ISingleton3>().To<Singleton3>().AsSingletonLazy();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        reflexContainer.Resolve<ISingleton1>();
                        reflexContainer.Resolve<ISingleton2>();
                        reflexContainer.Resolve<ISingleton3>();
                    }
                })
                .SampleGroup("Reflex")
                .GC()
                .Run();

            var builder = new ContainerBuilder();
            builder.Register<ISingleton1, Singleton1>(Lifetime.Singleton);
            builder.Register<ISingleton2, Singleton2>(Lifetime.Singleton);
            builder.Register<ISingleton3, Singleton3>(Lifetime.Singleton);
            var container = builder.Build();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        container.Resolve<ISingleton1>();
                        container.Resolve<ISingleton2>();
                        container.Resolve<ISingleton3>();
                    }
                })
                .SampleGroup("VContainer")
                .GC()
                .Run();
        }

        [Test]
        [Performance]
        public void ResolveTransient()
        {
            var zenjectContainer = new Zenject.DiContainer();
            zenjectContainer.Bind<ITransient1>().To<Transient1>().AsTransient();
            zenjectContainer.Bind<ITransient2>().To<Transient2>().AsTransient();
            zenjectContainer.Bind<ITransient3>().To<Transient3>().AsTransient();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        zenjectContainer.Resolve<ITransient1>();
                        zenjectContainer.Resolve<ITransient2>();
                        zenjectContainer.Resolve<ITransient3>();
                    }
                })
                .SampleGroup("Zenject")
                .GC()
                .Run();

            var reflexContainer = new Reflex.Container();
            reflexContainer.Bind<ITransient1>().To<Transient1>().AsTransient();
            reflexContainer.Bind<ITransient2>().To<Transient2>().AsTransient();
            reflexContainer.Bind<ITransient3>().To<Transient3>().AsTransient();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        reflexContainer.Resolve<ITransient1>();
                        reflexContainer.Resolve<ITransient2>();
                        reflexContainer.Resolve<ITransient3>();
                    }
                })
                .SampleGroup("Reflex")
                .GC()
                .Run();

            var builder = new ContainerBuilder();
            builder.Register<ITransient1, Transient1>(Lifetime.Transient);
            builder.Register<ITransient2, Transient2>(Lifetime.Transient);
            builder.Register<ITransient3, Transient3>(Lifetime.Transient);
            var container = builder.Build();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        container.Resolve<ITransient1>();
                        container.Resolve<ITransient2>();
                        container.Resolve<ITransient3>();
                    }
                })
                .SampleGroup("VContainer")
                .GC()
                .Run();
        }

        [Test]
        [Performance]
        public void ResolveCombined()
        {
            var zenjectContainer = new Zenject.DiContainer();
            zenjectContainer.Bind<ISingleton1>().To<Singleton1>().AsSingle();
            zenjectContainer.Bind<ISingleton2>().To<Singleton2>().AsSingle();
            zenjectContainer.Bind<ISingleton3>().To<Singleton3>().AsSingle();
            zenjectContainer.Bind<ITransient1>().To<Transient1>().AsTransient();
            zenjectContainer.Bind<ITransient2>().To<Transient2>().AsTransient();
            zenjectContainer.Bind<ITransient3>().To<Transient3>().AsTransient();
            zenjectContainer.Bind<ICombined1>().To<Combined1>().AsTransient();
            zenjectContainer.Bind<ICombined2>().To<Combined2>().AsTransient();
            zenjectContainer.Bind<ICombined3>().To<Combined3>().AsTransient();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        zenjectContainer.Resolve<ICombined1>();
                        zenjectContainer.Resolve<ICombined2>();
                        zenjectContainer.Resolve<ICombined3>();
                    }
                })
                .SampleGroup("Zenject")
                .GC()
                .Run();

            var reflexContainer = new Reflex.Container();
            reflexContainer.Bind<ISingleton1>().To<Singleton1>().AsSingletonLazy();
            reflexContainer.Bind<ISingleton2>().To<Singleton2>().AsSingletonLazy();
            reflexContainer.Bind<ISingleton3>().To<Singleton3>().AsSingletonLazy();
            reflexContainer.Bind<ITransient1>().To<Transient1>().AsTransient();
            reflexContainer.Bind<ITransient2>().To<Transient2>().AsTransient();
            reflexContainer.Bind<ITransient3>().To<Transient3>().AsTransient();
            reflexContainer.Bind<ICombined1>().To<Combined1>().AsTransient();
            reflexContainer.Bind<ICombined2>().To<Combined2>().AsTransient();
            reflexContainer.Bind<ICombined3>().To<Combined3>().AsTransient();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        reflexContainer.Resolve<ICombined1>();
                        reflexContainer.Resolve<ICombined2>();
                        reflexContainer.Resolve<ICombined3>();
                    }
                })
                .SampleGroup("Reflex")
                .GC()
                .Run();

            var builder = new ContainerBuilder();
            builder.Register<ISingleton1, Singleton1>(Lifetime.Singleton);
            builder.Register<ISingleton2, Singleton2>(Lifetime.Singleton);
            builder.Register<ISingleton3, Singleton3>(Lifetime.Singleton);
            builder.Register<ITransient1, Transient1>(Lifetime.Transient);
            builder.Register<ITransient2, Transient2>(Lifetime.Transient);
            builder.Register<ITransient3, Transient3>(Lifetime.Transient);
            builder.Register<ICombined1, Combined1>(Lifetime.Transient);
            builder.Register<ICombined2, Combined2>(Lifetime.Transient);
            builder.Register<ICombined3, Combined3>(Lifetime.Transient);
            var container = builder.Build();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        container.Resolve<ICombined1>();
                        container.Resolve<ICombined2>();
                        container.Resolve<ICombined3>();
                    }
                })
                .SampleGroup("VContainer")
                .GC()
                .Run();
        }

        [Test]
        [Performance]
        public void ResolveComplex()
        {
            var zenjectContainer = new Zenject.DiContainer();
            zenjectContainer.Bind<IFirstService>().To<FirstService>().AsSingle();
            zenjectContainer.Bind<ISecondService>().To<SecondService>().AsSingle();
            zenjectContainer.Bind<IThirdService>().To<ThirdService>().AsSingle();
            zenjectContainer.Bind<ISubObjectA>().To<SubObjectA>().AsTransient();
            zenjectContainer.Bind<ISubObjectB>().To<SubObjectB>().AsTransient();
            zenjectContainer.Bind<ISubObjectC>().To<SubObjectC>().AsTransient();
            zenjectContainer.Bind<IComplex1>().To<Complex1>().AsTransient();
            zenjectContainer.Bind<IComplex2>().To<Complex2>().AsTransient();
            zenjectContainer.Bind<IComplex3>().To<Complex3>().AsTransient();
            zenjectContainer.Bind<ISubObjectOne>().To<SubObjectOne>().AsTransient();
            zenjectContainer.Bind<ISubObjectTwo>().To<SubObjectTwo>().AsTransient();
            zenjectContainer.Bind<ISubObjectThree>().To<SubObjectThree>().AsTransient();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        zenjectContainer.Resolve<IComplex1>();
                        zenjectContainer.Resolve<IComplex2>();
                        zenjectContainer.Resolve<IComplex3>();
                    }
                })
                .SampleGroup("Zenject")
                .GC()
                .Run();

            var reflexContainer = new Reflex.Container();
            reflexContainer.Bind<IFirstService>().To<FirstService>().AsSingletonLazy();
            reflexContainer.Bind<ISecondService>().To<SecondService>().AsSingletonLazy();
            reflexContainer.Bind<IThirdService>().To<ThirdService>().AsSingletonLazy();
            reflexContainer.Bind<ISubObjectA>().To<SubObjectA>().AsTransient();
            reflexContainer.Bind<ISubObjectB>().To<SubObjectB>().AsTransient();
            reflexContainer.Bind<ISubObjectC>().To<SubObjectC>().AsTransient();
            reflexContainer.Bind<IComplex1>().To<Complex1>().AsTransient();
            reflexContainer.Bind<IComplex2>().To<Complex2>().AsTransient();
            reflexContainer.Bind<IComplex3>().To<Complex3>().AsTransient();
            reflexContainer.Bind<ISubObjectOne>().To<SubObjectOne>().AsTransient();
            reflexContainer.Bind<ISubObjectTwo>().To<SubObjectTwo>().AsTransient();
            reflexContainer.Bind<ISubObjectThree>().To<SubObjectThree>().AsTransient();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        reflexContainer.Resolve<IComplex1>();
                        reflexContainer.Resolve<IComplex2>();
                        reflexContainer.Resolve<IComplex3>();
                    }
                })
                .SampleGroup("Reflex")
                .GC()
                .Run();

            var builder = new ContainerBuilder();
            builder.Register<IFirstService, FirstService>(Lifetime.Singleton);
            builder.Register<ISecondService, SecondService>(Lifetime.Singleton);
            builder.Register<IThirdService, ThirdService>(Lifetime.Singleton);
            builder.Register<ISubObjectA, SubObjectA>(Lifetime.Transient);
            builder.Register<ISubObjectB, SubObjectB>(Lifetime.Transient);
            builder.Register<ISubObjectC, SubObjectC>(Lifetime.Transient);
            builder.Register<IComplex1, Complex1>(Lifetime.Transient);
            builder.Register<IComplex2, Complex2>(Lifetime.Transient);
            builder.Register<IComplex3, Complex3>(Lifetime.Transient);
            builder.Register<ISubObjectOne, SubObjectOne>(Lifetime.Transient);
            builder.Register<ISubObjectTwo, SubObjectTwo>(Lifetime.Transient);
            builder.Register<ISubObjectThree, SubObjectThree>(Lifetime.Transient);
            var container = builder.Build();

            Measure
                .Method(() =>
                {
                    // UnityEngine.Profiling.Profiler.BeginSample("VContainer Resolve(Complex)");
                    for (var i = 0; i < N; i++)
                    {
                        container.Resolve<IComplex1>();
                        container.Resolve<IComplex2>();
                        container.Resolve<IComplex3>();
                    }
                    // UnityEngine.Profiling.Profiler.EndSample();
                })
                .SampleGroup("VContainer")
                .GC()
                .Run();
        }

        // [Test]
        // [Performance]
        // public void ResolveComplex_VContainer()
        // {
        //     var builder = new ContainerBuilder();
        //     builder.Register<IFirstService, FirstService>(Lifetime.Singleton);
        //     builder.Register<ISecondService, SecondService>(Lifetime.Singleton);
        //     builder.Register<IThirdService, ThirdService>(Lifetime.Singleton);
        //     builder.Register<ISubObjectA, SubObjectA>(Lifetime.Transient);
        //     builder.Register<ISubObjectB, SubObjectB>(Lifetime.Transient);
        //     builder.Register<ISubObjectC, SubObjectC>(Lifetime.Transient);
        //     builder.Register<IComplex1, Complex1>(Lifetime.Transient);
        //     builder.Register<IComplex2, Complex2>(Lifetime.Transient);
        //     builder.Register<IComplex3, Complex3>(Lifetime.Transient);
        //     builder.Register<ISubObjectOne, SubObjectOne>(Lifetime.Transient);
        //     builder.Register<ISubObjectTwo, SubObjectTwo>(Lifetime.Transient);
        //     builder.Register<ISubObjectThree, SubObjectThree>(Lifetime.Transient);
        //     var container = builder.Build();
        //
        //     Measure
        //         .Method(() =>
        //         {
        //             UnityEngine.Profiling.Profiler.BeginSample("VContainer");
        //             container.Resolve<IComplex1>();
        //             container.Resolve<IComplex2>();
        //             container.Resolve<IComplex3>();
        //             UnityEngine.Profiling.Profiler.EndSample();
        //         })
        //         .Run();
        // }

        [Test]
        [Performance]
        public void ContainerBuildComplex()
        {
            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        var zenjectContainer = new Zenject.DiContainer();
                        zenjectContainer.Bind<IFirstService>().To<FirstService>().AsSingle();
                        zenjectContainer.Bind<ISecondService>().To<SecondService>().AsSingle();
                        zenjectContainer.Bind<IThirdService>().To<ThirdService>().AsSingle();
                        zenjectContainer.Bind<ISubObjectA>().To<SubObjectA>().AsTransient();
                        zenjectContainer.Bind<ISubObjectB>().To<SubObjectB>().AsTransient();
                        zenjectContainer.Bind<ISubObjectC>().To<SubObjectC>().AsTransient();
                        zenjectContainer.Bind<IComplex1>().To<Complex1>().AsTransient();
                        zenjectContainer.Bind<IComplex2>().To<Complex2>().AsTransient();
                        zenjectContainer.Bind<IComplex3>().To<Complex3>().AsTransient();
                        zenjectContainer.Bind<ISubObjectOne>().To<SubObjectOne>().AsTransient();
                        zenjectContainer.Bind<ISubObjectTwo>().To<SubObjectTwo>().AsTransient();
                        zenjectContainer.Bind<ISubObjectThree>().To<SubObjectThree>().AsTransient();
                    }
                })
                .SampleGroup("Zenject")
                .GC()
                .Run();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        var reflexContainer = new Reflex.Container();
                        reflexContainer.Bind<IFirstService>().To<FirstService>().AsSingletonLazy();
                        reflexContainer.Bind<ISecondService>().To<SecondService>().AsSingletonLazy();
                        reflexContainer.Bind<IThirdService>().To<ThirdService>().AsSingletonLazy();
                        reflexContainer.Bind<ISubObjectA>().To<SubObjectA>().AsTransient();
                        reflexContainer.Bind<ISubObjectB>().To<SubObjectB>().AsTransient();
                        reflexContainer.Bind<ISubObjectC>().To<SubObjectC>().AsTransient();
                        reflexContainer.Bind<IComplex1>().To<Complex1>().AsTransient();
                        reflexContainer.Bind<IComplex2>().To<Complex2>().AsTransient();
                        reflexContainer.Bind<IComplex3>().To<Complex3>().AsTransient();
                        reflexContainer.Bind<ISubObjectOne>().To<SubObjectOne>().AsTransient();
                        reflexContainer.Bind<ISubObjectTwo>().To<SubObjectTwo>().AsTransient();
                        reflexContainer.Bind<ISubObjectThree>().To<SubObjectThree>().AsTransient();
                    }
                })
                .SampleGroup("Reflex")
                .GC()
                .Run();

            Measure
                .Method(() =>
                {
                    for (var i = 0; i < N; i++)
                    {
                        var builder = new ContainerBuilder();
                        builder.Register<IFirstService, FirstService>(Lifetime.Singleton);
                        builder.Register<ISecondService, SecondService>(Lifetime.Singleton);
                        builder.Register<IThirdService, ThirdService>(Lifetime.Singleton);
                        builder.Register<ISubObjectA, SubObjectA>(Lifetime.Transient);
                        builder.Register<ISubObjectB, SubObjectB>(Lifetime.Transient);
                        builder.Register<ISubObjectC, SubObjectC>(Lifetime.Transient);
                        builder.Register<IComplex1, Complex1>(Lifetime.Transient);
                        builder.Register<IComplex2, Complex2>(Lifetime.Transient);
                        builder.Register<IComplex3, Complex3>(Lifetime.Transient);
                        builder.Register<ISubObjectOne, SubObjectOne>(Lifetime.Transient);
                        builder.Register<ISubObjectTwo, SubObjectTwo>(Lifetime.Transient);
                        builder.Register<ISubObjectThree, SubObjectThree>(Lifetime.Transient);
                        _ = builder.Build();
                    }
                })
                .SampleGroup("VContainer")
                .GC()
                .Run();
        }
    }
}
