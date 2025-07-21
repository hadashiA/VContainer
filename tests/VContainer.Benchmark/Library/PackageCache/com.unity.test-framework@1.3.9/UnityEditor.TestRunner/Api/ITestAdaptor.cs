using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// ```ITestAdaptor``` is a representation of a node in the test tree implemented as a wrapper around the [NUnit](http://www.nunit.org/) [ITest](https://github.com/nunit/nunit/blob/master/src/NUnitFramework/framework/Interfaces/ITest.cs)  interface.
    /// </summary>
    public interface ITestAdaptor
    {
        /// <summary>
        /// The ID of the test tree node. The ID can change if you add new tests to the suite. Use UniqueName, if you want to have a more permanent point of reference.
        /// </summary>
        string Id { get; }
        /// <summary>
        /// The name of the test. E.g.,```MyTest```.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The full name of the test. E.g., ```MyNamespace.MyTestClass.MyTest```.
        /// </summary>
        string FullName { get; }
        /// <summary>
        /// The total number of test cases in the node and all sub-nodes.
        /// </summary>
        int TestCaseCount { get; }
        /// <summary>
        /// Whether the node has any children.
        /// </summary>
        bool HasChildren { get; }
        /// <summary>
        /// True if the node is a test suite/fixture, false otherwise.
        /// </summary>
        bool IsSuite { get; }
        /// <summary>
        /// The child nodes.
        /// </summary>
        IEnumerable<ITestAdaptor> Children { get; }
        /// <summary>
        /// The parent node, if any.
        /// </summary>
        ITestAdaptor Parent { get; }
        /// <summary>
        /// The test case timeout in milliseconds. Note that this value is only available on TestFinished.
        /// </summary>
        int TestCaseTimeout { get; }
        /// <summary>
        /// The type of test class as an ```NUnit``` <see cref="ITypeInfo"/>. If the node is not a test class, then the value is null.
        /// </summary>
        ITypeInfo TypeInfo { get; }
        /// <summary>
        /// The Nunit <see cref="IMethodInfo"/> of the test method. If the node is not a test method, then the value is null.
        /// </summary>
        IMethodInfo Method { get; }
        /// <summary>
        /// The array of arguments that the test method/fixture will be invoked with.
        /// </summary>
        object[] Arguments { get; }
        /// <summary>
        /// An array of the categories applied to the test or fixture.
        /// </summary>
        string[] Categories { get; }
        /// <summary>
        /// Returns true if the node represents a test assembly, false otherwise.
        /// </summary>
        bool IsTestAssembly { get; }
        /// <summary>
        /// The run state of the test node. Either ```NotRunnable```, ```Runnable```, ```Explicit```, ```Skipped```, or ```Ignored```.
        /// </summary>
        RunState RunState { get; }
        /// <summary>
        /// The description of the test.
        /// </summary>
        string Description { get; }
        /// <summary>
        /// The skip reason. E.g., if ignoring the test.
        /// </summary>
        string SkipReason { get; }
        /// <summary>
        /// The ID of the parent node.
        /// </summary>
        string ParentId { get; }
        /// <summary>
        /// The full name of the parent node.
        /// </summary>
        string ParentFullName { get; }
        /// <summary>
        /// A unique generated name for the test node. E.g., ```Tests.dll/MyNamespace/MyTestClass/[Tests][MyNamespace.MyTestClass.MyTest]```.
        /// </summary>
        string UniqueName { get; }
        /// <summary>
        /// A unique name of the parent node. E.g., ```Tests.dll/MyNamespace/[Tests][MyNamespace.MyTestClass][suite]```.
        /// </summary>
        string ParentUniqueName { get; }
        /// <summary>
        /// The child index of the node in its parent.
        /// </summary>
        int ChildIndex { get; }
        /// <summary>
        /// The mode of the test. Either **Edit Mode** or **Play Mode**.
        /// </summary>
        TestMode TestMode { get; }
    }
}
