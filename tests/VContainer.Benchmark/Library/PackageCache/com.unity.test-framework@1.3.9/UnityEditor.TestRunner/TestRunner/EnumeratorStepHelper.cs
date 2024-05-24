using System;
using System.Collections;
using System.Reflection;

namespace UnityEditor.TestTools.TestRunner
{
    internal class EnumeratorStepHelper
    {
        private static int m_PC;

        public static void SetEnumeratorPC(int pc)
        {
            m_PC = pc;
        }

        /// <summary>
        /// Gets the current enumerator PC
        /// </summary>
        /// <returns>
        /// The PC
        /// 0 if no current Enumeration
        /// </returns>
        public static int GetEnumeratorPC(IEnumerator enumerator)
        {
            if (enumerator == null)
            {
                return 0;
            }
            return (int)GetPCFieldInfo(enumerator).GetValue(enumerator);
        }

        public static bool UpdateEnumeratorPcIfNeeded(IEnumerator enumerator)
        {
            if (m_PC != 0)
            {
                GetPCFieldInfo(enumerator).SetValue(enumerator, m_PC);
                m_PC = 0;
                return true;
            }
            return false;
        }

        private static FieldInfo GetPCFieldInfo(IEnumerator enumerator)
        {
            var field = enumerator.GetType().GetField("$PC", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) // Roslyn
                field = enumerator.GetType().GetField("<>1__state", BindingFlags.NonPublic | BindingFlags.Instance);

            return field;
        }
    }
}
