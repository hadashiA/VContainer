using System.Collections.Generic;

namespace VContainer.Unity
{
    sealed class ExistingComponentSpawner : IInstanceSpawner
    {
        readonly object instance;
        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> customParameters;

        public ExistingComponentSpawner(
            object instance,
            IInjector injector,
            IReadOnlyList<IInjectParameter> customParameters)
        {
            this.instance = instance;
            this.customParameters = customParameters;
            this.injector = injector;
        }

        public object Spawn(IObjectResolver resolver)
        {
            injector.Inject(instance, resolver, customParameters);
            return instance;
        }
    }
}