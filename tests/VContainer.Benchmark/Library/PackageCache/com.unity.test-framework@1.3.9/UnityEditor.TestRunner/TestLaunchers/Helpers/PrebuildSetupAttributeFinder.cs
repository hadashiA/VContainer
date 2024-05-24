using System;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal class PrebuildSetupAttributeFinder : AttributeFinderBase<IPrebuildSetup, PrebuildSetupAttribute>
    {
        public PrebuildSetupAttributeFinder() : base(attribute => attribute.TargetClass) {}
    }
}
