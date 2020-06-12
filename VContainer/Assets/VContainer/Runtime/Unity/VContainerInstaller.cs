using UnityEngine;

namespace VContainer.Unity
{
    public abstract class VContainerInstaller : MonoBehaviour
    {
        public abstract void Install(IContainerBuilder builder);
    }
}