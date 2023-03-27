using System.Collections.Generic;

namespace VContainer.Tests.LinkGenerator.Fixtures
{
    public class FooEmptyCtor
    {
        public FooEmptyCtor() { }
    }

    public class Foo1ParamCtor
    {
        public Foo1ParamCtor(FooEmptyCtor _) { }
    }

    public class FooManyParamCtor
    {
        public FooManyParamCtor(FooEmptyCtor _1,
            Dictionary<FooEmptyCtor, List<FooEmptyCtor>> _2,
            int? _3,
            float _4,
            string _5) { }
    }

    public class FooManyCtors
    {
        public FooManyCtors()
        {
        }
        
        [Inject]
        public FooManyCtors(FooEmptyCtor _1,
            Dictionary<FooEmptyCtor, List<FooEmptyCtor>> _2,
            int? _3,
            float _4,
            string _5) { }
    }

    public class FooFields
    {
        [Inject]
        public FooEmptyCtor foo1;
        public FooEmptyCtor foo2;
    }

    public class FooAutoProperty
    {
        [Inject]
        public FooEmptyCtor foo1 { get; set; }
    }

    public class FooMethod
    {
        [Inject]
        public void FooInject1() { }

        [Inject]
        public void FooInject2(FooEmptyCtor _1,
            Dictionary<FooEmptyCtor, List<FooEmptyCtor>> _2,
            int? _3,
            float _4,
            string _5) { }

        [Inject]
        public Dictionary<FooEmptyCtor, List<FooEmptyCtor>> FooInject3(FooEmptyCtor _1,
            Dictionary<FooEmptyCtor, List<FooEmptyCtor>> _2,
            int? _3,
            float _4,
            string _5) =>
            null;

        [Inject]
        public int? FooInject4() => null;
    }
}