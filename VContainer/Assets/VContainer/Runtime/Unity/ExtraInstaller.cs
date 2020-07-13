using System.Collections;
using System.Collections.Generic;

namespace VContainer.Unity
{
    public sealed class ExtraInstaller : IInstaller, IEnumerable<IInstaller>
    {
        readonly IList<IInstaller> extraInstallers = new List<IInstaller>();

        public IEnumerator<IInstaller> GetEnumerator() => extraInstallers.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(IInstaller installer) => extraInstallers.Add(installer);

        public void Install(IContainerBuilder builder)
        {
            foreach (var installer in extraInstallers)
            {
                installer.Install(builder);
            }
        }
    }
}