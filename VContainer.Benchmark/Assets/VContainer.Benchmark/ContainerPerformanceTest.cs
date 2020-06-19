using NUnit.Framework;
using Unity.PerformanceTesting;
using VContainer;
using VContainer.Benchmark.Fixtures;
using Zenject;

namespace Vcontainer.Benchmark
{
    public class ContainerPerformanceTest
    {
        [Test]
        [Performance]
        public void ResolveSingleton()
        {
            var zenjectContainer = new DiContainer();
            zenjectContainer.Bind<ISingleton1>().To<Singleton1>().AsSingle();
            zenjectContainer.Bind<ISingleton2>().To<Singleton2>().AsSingle();
            zenjectContainer.Bind<ISingleton3>().To<Singleton3>().AsSingle();

            Measure
                .Method(() =>
                {
                    zenjectContainer.Resolve<ISingleton1>();
                    zenjectContainer.Resolve<ISingleton2>();
                    zenjectContainer.Resolve<ISingleton3>();
                })
                .SampleGroup("Zenject")
                .WarmupCount(100)
                .MeasurementCount(100)
                .Run();

            var builder = new ContainerBuilder();
            builder.Register<ISingleton1, Singleton1>(Lifetime.Singleton);
            builder.Register<ISingleton2, Singleton2>(Lifetime.Singleton);
            builder.Register<ISingleton3, Singleton3>(Lifetime.Singleton);
            var container = builder.Build();

            Measure
                .Method(() =>
                {
                    container.Resolve<ISingleton1>();
                    container.Resolve<ISingleton2>();
                    container.Resolve<ISingleton3>();
                })
                .SampleGroup("VContainer")
                .WarmupCount(100)
                .MeasurementCount(100)
                .Run();
        }

        [Test]
        [Performance]
        public void ResolveComplex()
        {
            var zenjectContainer = new DiContainer();
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
                    UnityEngine.Profiling.Profiler.BeginSample("Zenject.ResolveComplex");
                    zenjectContainer.Resolve<IComplex1>();
                    zenjectContainer.Resolve<IComplex2>();
                    zenjectContainer.Resolve<IComplex3>();
                    UnityEngine.Profiling.Profiler.EndSample();
                })
                .SampleGroup("Zenject")
                .WarmupCount(100)
                .MeasurementCount(100)
                // .GC()
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
                    UnityEngine.Profiling.Profiler.BeginSample("VContainer.ResolveComplex");
                    container.Resolve<IComplex1>();
                    container.Resolve<IComplex2>();
                    container.Resolve<IComplex3>();
                    UnityEngine.Profiling.Profiler.EndSample();
                })
                .SampleGroup("VContainer")
                .WarmupCount(100)
                .MeasurementCount(100)
                // .GC()
                .Run();
        }
    }
}
