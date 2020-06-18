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
        IInjector Build(Type type, bool skipConstructor);
    }

    class NullInjector : IInjector
    {
        public readonly object instance;

        public NullInjector(object instance)
        {
            this.instance = instance;
        }

        public void Inject(object _, IObjectResolver resolver)
        {
        }

        public object CreateInstance(IObjectResolver resolver) => instance;
    }
}
