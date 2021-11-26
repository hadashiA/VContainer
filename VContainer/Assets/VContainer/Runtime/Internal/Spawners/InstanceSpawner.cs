using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer.Internal.Spawners
{
    public sealed class InstanceSpawner : IInstanceSpawner
    {
        readonly IInjector injector;
        readonly IReadOnlyList<IInjectParameter> customParameters;

        public InstanceSpawner(
            IInjector injector,
            IReadOnlyList<IInjectParameter> customParameters = null)
        {
            this.injector = injector;
            this.customParameters = customParameters;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Spawn(IObjectResolver resolver)
            => injector.CreateInstance(resolver, customParameters);
    }
}