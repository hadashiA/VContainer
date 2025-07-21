using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// `UnityTest` attribute is the main addition to the standard [NUnit](http://www.nunit.org/) library for the Unity Test Framework. This type of unit test allows you to skip a frame from within a test (so background tasks can finish) or give certain commands to the Unity **Editor**, such as performing a domain reload or entering **Play Mode** from an **Edit Mode** test.
    /// In Play Mode, the `UnityTest` attribute runs as a [coroutine](https://docs.unity3d.com/Manual/Coroutines.html). Whereas Edit Mode tests run in the [EditorApplication.update](https://docs.unity3d.com/ScriptReference/EditorApplication-update.html) callback loop.
    /// The `UnityTest` attribute is, in fact, an alternative to the `NUnit` [Test attribute](https://github.com/nunit/docs/wiki/Test-Attribute), which allows yielding instructions back to the framework. Once the instruction is complete, the test run continues. If you `yield return null`, you skip a frame. That might be necessary to ensure that some changes do happen on the next iteration of either the `EditorApplication.update` loop or the [game loop](https://docs.unity3d.com/Manual/ExecutionOrder.html).
    /// <example>
    /// ## Edit Mode example
    /// The most simple example of an Edit Mode test could be the one that yields `null` to skip the current frame and then continues to run:
    /// <code>
    /// [UnityTest]
    /// public IEnumerator EditorUtility_WhenExecuted_ReturnsSuccess()
    /// {
    ///     var utility = RunEditorUtilityInTheBackground();
    ///
    ///     while (utility.isRunning)
    ///     {
    ///         yield return null;
    ///     }
    ///
    ///     Assert.IsTrue(utility.isSuccess);
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// ## Play Mode example
    ///
    /// In Play Mode, a test runs as a coroutine attached to a [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html). So all the yield instructions available in coroutines, are also available in your test.
    ///
    /// From a Play Mode test you can use one of Unity’s [Yield Instructions](https://docs.unity3d.com/ScriptReference/YieldInstruction.html):
    ///
    /// - [WaitForFixedUpdate](https://docs.unity3d.com/ScriptReference/WaitForFixedUpdate.html): to ensure changes expected within the next cycle of physics calculations.
    /// - [WaitForSeconds](https://docs.unity3d.com/ScriptReference/WaitForSeconds.html): if you want to pause your test coroutine for a fixed amount of time. Be careful about creating long-running tests.
    ///
    /// The simplest example is to yield to `WaitForFixedUpdate`:
    /// <code>
    /// [UnityTest]
    /// public IEnumerator GameObject_WithRigidBody_WillBeAffectedByPhysics()
    /// {
    ///     var go = new GameObject();
    ///     go.AddComponent&lt;Rigidbody&gt;();
    ///     var originalPosition = go.transform.position.y;
    ///
    ///     yield return new WaitForFixedUpdate();
    ///
    ///     Assert.AreNotEqual(originalPosition, go.transform.position.y);
    /// }
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class UnityTestAttribute : CombiningStrategyAttribute, IImplyFixture, ISimpleTestBuilder, ITestBuilder, IApplyToTest
    {
        private const string k_MethodMarkedWithUnitytestMustReturnIenumerator = "Method marked with UnityTest must return IEnumerator.";

        /// <summary>
        /// Initializes and returns an instance of UnityTestAttribute.
        /// </summary>
        public UnityTestAttribute() : base(new UnityCombinatorialStrategy(), new ParameterDataSourceProvider()) {}

        private readonly NUnitTestCaseBuilder _builder = new NUnitTestCaseBuilder();

        /// <summary>
        /// This method builds the TestMethod from the Test and the method info. In addition it removes the expected result of the test.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <param name="suite">The test.</param>
        /// <returns>A TestMethod object</returns>
        TestMethod ISimpleTestBuilder.BuildFrom(IMethodInfo method, Test suite)
        {
            var t = CreateTestMethod(method, suite);

            AdaptToUnityTestMethod(t);

            return t;
        }

        /// <summary>
        /// This method hides the base method from CombiningStrategyAttribute.
        /// It builds a TestMethod from a Parameterized Test and the method info.
        /// In addition it removes the expected result of the test.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <param name="suite">The test.</param>
        /// <returns>A TestMethod object</returns>
        IEnumerable<TestMethod> ITestBuilder.BuildFrom(IMethodInfo method, Test suite)
        {
            var testMethods  = base.BuildFrom(method, suite);

            foreach (var t in testMethods)
            {
                AdaptToUnityTestMethod(t);
            }

            return testMethods;
        }

        private TestMethod CreateTestMethod(IMethodInfo method, Test suite)
        {
            TestCaseParameters parms = new TestCaseParameters
            {
                ExpectedResult = new object(),
                HasExpectedResult = true
            };

            var t = _builder.BuildTestMethod(method, suite, parms);
            return t;
        }

        private static void AdaptToUnityTestMethod(TestMethod t)
        {
            if (t.parms != null)
            {
                t.parms.HasExpectedResult = false;
            }
        }

        private static bool IsMethodReturnTypeIEnumerator(IMethodInfo method)
        {
            return !method.ReturnType.IsType(typeof(IEnumerator));
        }

        /// <summary>
        /// This method hides the base method ApplyToTest from CombiningStrategyAttribute.
        /// In addition it ensures that the test with the `UnityTestAttribute` has an IEnumerator as return type.
        /// </summary>
        /// <param name="test">The test.</param>
        public new void ApplyToTest(Test test)
        {
            if (IsMethodReturnTypeIEnumerator(test.Method))
            {
                test.RunState = RunState.NotRunnable;
                test.Properties.Set(PropertyNames.SkipReason, k_MethodMarkedWithUnitytestMustReturnIenumerator);
            }

            base.ApplyToTest(test);
        }
    }
}
