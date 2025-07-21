using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    // This matches the state definitions expected by the Perl code, which in turn matches the NUnit 2 values...
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
