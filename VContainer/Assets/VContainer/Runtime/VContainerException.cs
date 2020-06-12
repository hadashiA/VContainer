using System;

namespace VContainer
{
    public sealed class VContainerException : Exception
    {
        public VContainerException(string message) : base(message)
        {
        }
    }
}
