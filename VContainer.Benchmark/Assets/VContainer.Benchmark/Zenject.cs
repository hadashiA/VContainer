using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;
using VContainer.Benchmark.Fixtures;
using Zenject;

namespace Vcontainer.Benchmark
{
    public class Zenject
    {
        [Test]
        [Performance]
        public void ResolveSingleton()
        {
            var container = new DiContainer();
            container.Bind<ISingleton1>().To<Singleton1>().AsSingle();
            container.Bind<ISingleton2>().To<Singleton2>().AsSingle();
            container.Bind<ISingleton3>().To<Singleton3>().AsSingle();

            Measure
                .Method(() =>
                {
                    container.Resolve<ISingleton1>();
                    container.Resolve<ISingleton2>();
                    container.Resolve<ISingleton3>();
                })
                .WarmupCount(10)
                .MeasurementCount(10)
                .Run();
        }

        [Test]
        [Performance]
        public void ResolveComplex()
        {
            var container = new DiContainer();
            container.Bind<IFirstService>().To<FirstService>().AsSingle();
            container.Bind<ISecondService>().To<SecondService>().AsSingle();
            container.Bind<IThirdService>().To<ThirdService>().AsSingle();
            container.Bind<ISubObjectA>().To<SubObjectA>().AsTransient();
            container.Bind<ISubObjectB>().To<SubObjectB>().AsTransient();
            container.Bind<ISubObjectC>().To<SubObjectC>().AsTransient();
            container.Bind<IComplex1>().To<Complex1>().AsTransient();
            container.Bind<IComplex2>().To<Complex2>().AsTransient();
            container.Bind<IComplex3>().To<Complex3>().AsTransient();
            container.Bind<ISubObjectOne>().To<SubObjectOne>().AsTransient();
            container.Bind<ISubObjectTwo>().To<SubObjectTwo>().AsTransient();
            container.Bind<ISubObjectThree>().To<SubObjectThree>().AsTransient();

            Measure
                .Method(() =>
                {
                    UnityEngine.Profiling.Profiler.BeginSample("Zenject.ResolveComplex");
                    container.Resolve<IComplex1>();
                    container.Resolve<IComplex2>();
                    container.Resolve<IComplex3>();
                    UnityEngine.Profiling.Profiler.EndSample();
                })
                .WarmupCount(100)
                .MeasurementCount(100)
                .Run();
        }
    }
}
