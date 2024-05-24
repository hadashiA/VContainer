using UnityEngine;

namespace Reflex.Scripts
{
    public abstract class MonoInstaller : MonoBehaviour
    {
        public abstract void InstallBindings(Container container);
    }
}