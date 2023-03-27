using System;

namespace VContainer.Tests.LinkGenerator
{
    [Serializable]
    public abstract class BaseFoo
    {
    }

    [Serializable]
    public class DerivedBaseFoo1 : BaseFoo
    {
        public int test;
    }

    [Serializable]
    public class DerivedBaseFoo2 : BaseFoo
    {
        public int test;
    }

    [Serializable]
    public class FooSerializable1
    {
        public int fooField;
    }
    
    [Serializable]
    public class FooSerializable2
    {
        public int fooField;
    }
}