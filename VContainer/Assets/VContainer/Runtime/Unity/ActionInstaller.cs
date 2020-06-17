using System;

namespace VContainer.Unity
{
    public sealed class ActionInstaller : IInstaller
    {
        public static implicit operator ActionInstaller(Action<ContainerBuilderUnity> installation)
            => new ActionInstaller(installation);

        readonly Action<ContainerBuilderUnity> configuration;

        public ActionInstaller(Action<ContainerBuilderUnity> configuration)
        {
            this.configuration = configuration;
        }

        public void Install(ContainerBuilderUnity builder)
        {
            configuration(builder);
        }
    }
}