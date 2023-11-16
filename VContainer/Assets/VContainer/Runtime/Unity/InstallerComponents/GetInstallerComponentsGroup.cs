using System.Diagnostics;
using UnityEngine;

namespace VContainer.Unity
{
    [DisallowMultipleComponent]
    public class GetInstallerComponentsGroup : InstallerComponent
    {
        [SerializeField] bool active = true;
        [SerializeField] GameObject target;

        public override void Install(IContainerBuilder builder)
        {
            if (!active)
            {
                return;
            }

            if (target == null)
            {
                target = gameObject;
            }

            foreach (var installer in GetComponents<IInstaller>())
            {
                if(installer != null && !ReferenceEquals(installer, this))
                {
                    installer.Install(builder);
                }
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void Reset()
        {
            target = gameObject;
        }
    }
}