using System;

namespace Reflex
{
    internal class Binding
    {
        internal Type Contract;
        internal Type Concrete;
        internal BindingScope Scope;
        internal Func<object> Method;
    }
}