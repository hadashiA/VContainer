using System;
using System.Collections.Generic;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class TestPlanMessage : Message
    {
        public List<string> tests;

        public TestPlanMessage()
        {
            type = "TestPlan";
        }
    }
}
