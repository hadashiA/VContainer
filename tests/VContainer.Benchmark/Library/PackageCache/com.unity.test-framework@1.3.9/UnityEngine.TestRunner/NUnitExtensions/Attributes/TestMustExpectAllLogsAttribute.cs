using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// The presence of this attribute will cause the test runner to require that every single log is expected. By
    /// default, the runner will only automatically fail on any error logs, so this adds warnings and infos as well.
    /// It is the same as calling `LogAssert.NoUnexpectedReceived()` at the bottom of every affected test.
    ///
    /// This attribute can be applied to test assemblies (will affect every test in the assembly), fixtures (will
    /// affect every test in the fixture), or on individual test methods. It is also automatically inherited from base
    /// fixtures.
    ///
    /// The MustExpect property (on by default) lets you selectively enable or disable the higher level value. For
    /// example when migrating an assembly to this more strict checking method, you might attach
    /// `[assembly:TestMustExpectAllLogs]` to the assembly itself, but then whitelist failing fixtures and test methods
    /// with `[TestMustExpectAllLogs(MustExpect=false)]` until they can be migrated. This also means new tests in that
    /// assembly would be required to have the more strict checking. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public class TestMustExpectAllLogsAttribute : Attribute
    {
        /// <summary>
        /// Initializes and returns an instance of TestMustExpectAllLogsAttribute.
        /// </summary>
        /// <param name="mustExpect">
        /// A value indicating whether the test must expect all logs.
        /// </param>
        public TestMustExpectAllLogsAttribute(bool mustExpect = true)
            => MustExpect = mustExpect;
        /// <summary>
        /// Returns the flag of whether the test must expect all logs.
        /// </summary>
        public bool MustExpect { get; }
    }
}
