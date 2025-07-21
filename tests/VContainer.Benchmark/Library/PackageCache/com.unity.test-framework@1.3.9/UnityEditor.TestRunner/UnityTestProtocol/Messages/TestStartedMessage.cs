using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class TestStartedMessage : Message
    {
        public string name;
        public TestState state;

        public TestStartedMessage()
        {
            type = "TestStatus";
            phase = "Begin";
            state = TestState.Inconclusive;
        }
    }
}
