using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    public static class RaycasterManager
    {
        private static readonly List<BaseRaycaster> s_Raycasters = new List<BaseRaycaster>();

        internal static void AddRaycaster(BaseRaycaster baseRaycaster)
        {
            if (s_Raycasters.Contains(baseRaycaster))
                return;

            s_Raycasters.Add(baseRaycaster);
        }

        /// <summary>
        /// List of BaseRaycasters that has been registered.
        /// </summary>
        public static List<BaseRaycaster> GetRaycasters()
        {
            return s_Raycasters;
        }

        internal static void RemoveRaycasters(BaseRaycaster baseRaycaster)
        {
            if (!s_Raycasters.Contains(baseRaycaster))
                return;
            s_Raycasters.Remove(baseRaycaster);
        }
    }
}
