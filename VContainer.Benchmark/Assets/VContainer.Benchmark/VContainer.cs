using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;
using VContainer;
using VContainer.Benchmark.Fixtures;

namespace Vcontainer.Benchmark
{
    public class VContainer
    {
        [Test]
        [Performance]
        public void ResolveSingleton()
        {
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
                .ProfilerMarkers()
                .Run();
        }

        [Test]
        [Performance]
        public void ResolveComplex()
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
                .WarmupCount(10)
                .MeasurementCount(10)
                .Run();
        }
    }
}
