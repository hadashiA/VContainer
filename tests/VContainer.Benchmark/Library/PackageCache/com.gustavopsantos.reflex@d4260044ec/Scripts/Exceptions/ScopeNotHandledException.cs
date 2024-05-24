using System;

namespace Reflex
{
    internal class ScopeNotHandledException : Exception
    {
        public ScopeNotHandledException(string message) : base (message) { }
    }
}

