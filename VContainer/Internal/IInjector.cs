using System;

namespace VContainer
{
    interface IInjector
    {
        void Inject(object instance, IObjectResolver resolver);
        object CreateInstance(IObjectResolver resolver);
    }

    interface IInjectorBuilder
    {
        IInjector Build(Type type);
    }
}
