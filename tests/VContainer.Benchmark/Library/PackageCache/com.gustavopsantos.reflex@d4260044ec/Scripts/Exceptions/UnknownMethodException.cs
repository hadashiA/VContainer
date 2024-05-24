using System;

namespace Reflex
{
    internal class UnknownMethodException : Exception
    {
        public UnknownMethodException(string message) : base(message) { }
    }
}