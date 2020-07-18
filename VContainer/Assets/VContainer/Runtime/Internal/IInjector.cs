using System;
using System.Collections.Generic;
using VContainer.Internal;

namespace VContainer
{
    public interface IInjector
    {
        void Inject(object instance, IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters);
        object CreateInstance(IObjectResolver resolver, IReadOnlyList<IInjectParameter> parameters);
    }

    interface IInjectorBuilder
    {
        IInjector Build(Type type);
    }
}
