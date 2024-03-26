using UnityEngine;

namespace VContainer.Unity
{
    public abstract class BuildCallbackInstaller : InstallerComponent
    {
        [SerializeField] bool active = true;
        
        protected abstract void OnBuild(IObjectResolver resolver);

        public override void Install(IContainerBuilder builder)
        {
            if(!active)
            {
                return;
            }

            builder.RegisterBuildCallback(OnBuild);
        }
    }
}