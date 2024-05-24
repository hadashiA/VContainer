namespace UnityEngine.TestRunner.TestProtocol
{
    internal class TestFinishedMessage : MessageForRetryRepeat
    {
        public string name;
        public TestState state;
        public string message;
        public ulong duration; // milliseconds
        public ulong durationMicroseconds;
        public string stackTrace;

        public TestFinishedMessage()
        {
            type = "TestStatus";
            phase = "End";
        }
    }
}