using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner
{
    [Serializable]
    internal class PlatformSpecificSetup
    {
        [SerializeField]
        private ApplePlatformSetup m_AppleiOSPlatformSetup = new ApplePlatformSetup(BuildTarget.iOS);
        [SerializeField]
        private ApplePlatformSetup m_AppleTvOSPlatformSetup = new ApplePlatformSetup(BuildTarget.tvOS);
#if !UNITY_2021_1_OR_NEWER
        [SerializeField]
        private XboxOnePlatformSetup m_XboxOnePlatformSetup = new XboxOnePlatformSetup();
#endif
        [SerializeField]
        private AndroidPlatformSetup m_AndroidPlatformSetup = new AndroidPlatformSetup();
        [SerializeField]
        private SwitchPlatformSetup m_SwitchPlatformSetup = new SwitchPlatformSetup();
#if UNITY_2019_3_OR_NEWER
        [SerializeField]
        private StadiaPlatformSetup m_StadiaPlatformSetup = new StadiaPlatformSetup();
#endif
        [SerializeField]
        private UwpPlatformSetup m_UwpPlatformSetup = new UwpPlatformSetup();

        [SerializeField]
        private LuminPlatformSetup m_LuminPlatformSetup = new LuminPlatformSetup();


        private IDictionary<BuildTarget, IPlatformSetup> m_SetupTypes;

        [SerializeField]
        private BuildTarget m_Target;

        public PlatformSpecificSetup()
        {
        }

        public PlatformSpecificSetup(BuildTarget target)
        {
            m_Target = target;
        }

        public void Setup()
        {
            var dictionary = GetSetup();

            if (!dictionary.ContainsKey(m_Target))
            {
                return;
            }

            dictionary[m_Target].Setup();
        }

        public void PostBuildAction()
        {
            var dictionary = GetSetup();

            if (!dictionary.ContainsKey(m_Target))
            {
                return;
            }

            dictionary[m_Target].PostBuildAction();
        }

        public void PostSuccessfulBuildAction()
        {
            var dictionary = GetSetup();

            if (!dictionary.ContainsKey(m_Target))
            {
                return;
            }

            dictionary[m_Target].PostSuccessfulBuildAction();
        }

        public void PostSuccessfulLaunchAction()
        {
            var dictionary = GetSetup();

            if (!dictionary.ContainsKey(m_Target))
            {
                return;
            }

            dictionary[m_Target].PostSuccessfulLaunchAction();
        }

        public void CleanUp()
        {
            var dictionary = GetSetup();

            if (!dictionary.ContainsKey(m_Target))
            {
                return;
            }

            dictionary[m_Target].CleanUp();
        }

        private IDictionary<BuildTarget, IPlatformSetup> GetSetup()
        {
            m_SetupTypes = new Dictionary<BuildTarget, IPlatformSetup>
            {
                {BuildTarget.iOS, m_AppleiOSPlatformSetup},
                {BuildTarget.tvOS, m_AppleTvOSPlatformSetup},
#if !UNITY_2021_1_OR_NEWER
                {BuildTarget.XboxOne, m_XboxOnePlatformSetup},
#endif
                {BuildTarget.Android, m_AndroidPlatformSetup},
                {BuildTarget.WSAPlayer, m_UwpPlatformSetup},
#if !UNITY_2022_2_OR_NEWER
                {BuildTarget.Lumin, m_LuminPlatformSetup},
#endif
#if UNITY_2019_3_OR_NEWER && !UNITY_2023_1_OR_NEWER
                {BuildTarget.Stadia, m_StadiaPlatformSetup},
#endif
                {BuildTarget.Switch, m_SwitchPlatformSetup}
            };
            return m_SetupTypes;
        }
    }
}
