using UnityEngine;

namespace VContainer.Unity
{
    public abstract class MonoInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(ContainerBuilderUnity builder);
    }
}
