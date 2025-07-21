using System;

namespace Unity.PerformanceTesting.Exceptions
{
    [Serializable]
    public class PerformanceTestException : System.Exception
    {
        public PerformanceTestException()
            : base() { }

        public PerformanceTestException(string message)
            : base(message) { }

        public PerformanceTestException(string message, System.Exception inner)
            : base(message, inner) { }

        protected PerformanceTestException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
