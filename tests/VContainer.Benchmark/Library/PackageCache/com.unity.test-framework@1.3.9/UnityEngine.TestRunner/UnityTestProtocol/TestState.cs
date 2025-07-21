namespace UnityEngine.TestRunner.TestProtocol
{
    internal enum TestState
    {
        Inconclusive = 0,
        Skipped = 2,
        Ignored = 3,
        Success = 4,
        Failure = 5,
        Error = 6
    }
}