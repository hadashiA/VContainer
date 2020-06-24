using System;

namespace VContainer.Unity
{
    public sealed class ActionInstaller : IInstaller
    {
        public static implicit operator ActionInstaller(Action<IContainerBuilder> installation)
            => new ActionInstaller(installation);

        readonly Action<IContainerBuilder> configuration;

        public ActionInstaller(Action<IContainerBuilder> configuration)
        {
            this.configuration = configuration;
        }

        public void Install(IContainerBuilder builder)
        {
            configuration(builder);
        }
    }
}