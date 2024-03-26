using UnityEngine;

namespace VContainer.Unity
{
    public class InstallersGroup : InstallerComponent
    {
        [SerializeField] bool active = true;
        [SerializeField] InstallerComponent[] installerComponents;
        
        public override void Install(IContainerBuilder builder)
        {
            if(!active)
            {
                return;
            }
            
            foreach (var installer in installerComponents)
            {
                if(installer != null && !ReferenceEquals(installer, this))
                {
                    installer.Install(builder);
                }
            }
        }
    }
}