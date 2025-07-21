namespace UnityEngine.TestRunner.TestProtocol
{
    internal class TestStartedMessage : MessageForRetryRepeat
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