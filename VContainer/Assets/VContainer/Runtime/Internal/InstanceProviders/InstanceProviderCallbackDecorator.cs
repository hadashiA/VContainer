using System;

namespace VContainer.Internal
{
    sealed class InstanceProviderCallbackDecorator : IInstanceProvider
    {
        readonly Action<object, IObjectResolver> callback;
        readonly IInstanceProvider instanceProvider;

        public InstanceProviderCallbackDecorator(IInstanceProvider instanceProvider, Action<object, IObjectResolver> callback)
        {
            this.instanceProvider = instanceProvider;
            this.callback = callback;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var instance = instanceProvider.SpawnInstance(resolver);
            callback(instance, resolver);
            return instance;
        }
    }
}