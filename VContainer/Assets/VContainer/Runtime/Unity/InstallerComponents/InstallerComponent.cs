using UnityEngine;

namespace VContainer.Unity
{
    public abstract class InstallerComponent : MonoBehaviour, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}