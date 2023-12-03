using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer.Unity
{
    public static class InstallerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IContainerBuilder Register(this IContainerBuilder builder, IInstaller installer)
        {
            // ReSharper disable once UseNullPropagation (might be a component)
            if (installer != null)
            {
                installer.Install(builder);
            }
            
            return builder;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IContainerBuilder Register(this IContainerBuilder builder, IEnumerable<IInstaller> installers)
        {
            if (installers != null)
            {
                foreach (var installer in installers)
                {
                    builder.Register(installer);
                }
            }
            

            return builder;
        }
    }
}