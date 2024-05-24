using System;
using System.Collections;

namespace UnityEngine.TestTools
{
    internal class TestCommandPcHelper
    {
        public virtual void SetEnumeratorPC(IEnumerator enumerator, int pc)
        {
            // Noop implementation used in playmode.
        }

        public virtual int GetEnumeratorPC(IEnumerator enumerator)
        {
            return 0;
        }
    }
}
