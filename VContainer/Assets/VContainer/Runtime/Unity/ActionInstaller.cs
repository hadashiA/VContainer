using System;

namespace VContainer.Unity
{
    public sealed class ActionInstaller : IInstaller
    {
        public static implicit operator ActionInstaller(Action<UnityContainerBuilder> installation)
            => new ActionInstaller(installation);

        readonly Action<UnityContainerBuilder> configuration;

        public ActionInstaller(Action<UnityContainerBuilder> configuration)
        {
            this.configuration = configuration;
        }

        public void Install(UnityContainerBuilder builder)
        {
            configuration(builder);
        }
    }
}