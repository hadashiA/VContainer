using System;
using UnityEngine.Scripting;

namespace UnityEngine.TestRunner
{
    /// <summary>
    /// An assembly level attribute that indicates that a given type should be subscribed for receiving updates on the test progress.
    /// </summary>
    /// <example>
    /// <code>
    /// using NUnit.Framework.Interfaces;
    /// using UnityEngine;
    /// using UnityEngine.TestRunner;
    ///
    /// [assembly:TestRunCallback(typeof(TestListener))]
    ///
    /// public class TestListener : ITestRunCallback
    /// {
    ///    public void RunStarted(ITest testsToRun)
    ///    {
    ///
    ///    }
    ///
    ///    public void RunFinished(ITestResult testResults)
    ///    {
    ///        Debug.Log($"Run finished with result {testResults.ResultState}.");
    ///    }
    ///
    ///    public void TestStarted(ITest test)
    ///    {
    ///
    ///    }
    ///
    ///    public void TestFinished(ITestResult result)
    ///    {
    ///
    ///    }
    ///}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class TestRunCallbackAttribute : Attribute
    {
        private Type m_Type;

        /// <summary>
        /// Constructs a new instance of the <see cref="TestRunCallbackAttribute"/> class.
        /// </summary>
        /// <param name="type">A target type that implements <see cref="ITestRunCallback"/>.</param>
        /// <exception cref="ArgumentException">Throws an ArgumentException if the provided type does not implement <see cref="ITestRunCallback"/>.</exception>
        public TestRunCallbackAttribute(Type type)
        {
            var interfaceType = typeof(ITestRunCallback);
            if (!interfaceType.IsAssignableFrom(type))
            {
                throw new ArgumentException(string.Format(
                    "Type {2} provided to {0} does not implement {1}. If the stripping level is set to high, the implementing class should have the {3}.",
                    GetType().Name, interfaceType.Name, type.Name, typeof(PreserveAttribute).Name));
            }
            m_Type = type;
        }

        internal ITestRunCallback ConstructCallback()
        {
            return Activator.CreateInstance(m_Type) as ITestRunCallback;
        }
    }
}
