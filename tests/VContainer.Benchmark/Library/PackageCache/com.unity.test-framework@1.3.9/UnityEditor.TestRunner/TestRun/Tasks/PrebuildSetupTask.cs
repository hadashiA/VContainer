using System;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class PrebuildSetupTask : BuildActionTaskBase<IPrebuildSetup>
    {
        public PrebuildSetupTask() : base(new PrebuildSetupAttributeFinder())
        {
        }

        protected override void Action(IPrebuildSetup target)
        {
            target.Setup();
        }
    }
}
