using System;

namespace Reflex
{
    internal class UnknownContractException : Exception
    {
        public UnknownContractException(string message) : base(message) { }
    }
}