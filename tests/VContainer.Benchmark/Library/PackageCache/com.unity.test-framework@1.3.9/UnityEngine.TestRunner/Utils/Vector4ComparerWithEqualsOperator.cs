using System;
using System.Collections.Generic;

namespace UnityEngine.TestTools.Utils
{
    /// <summary>
    /// Use these classes to compare two objects of the same type for equality within the range of a given tolerance using NUnit or custom constraints . Call Instance to apply the default calculation error value to the comparison.
    /// </summary>
    public class Vector4ComparerWithEqualsOperator : IEqualityComparer<Vector4>
    {
        private static readonly Vector4ComparerWithEqualsOperator m_Instance = new Vector4ComparerWithEqualsOperator();
        /// <summary>
        /// A singleton instance of the comparer with a predefined default error value.
        /// </summary>
        public static Vector4ComparerWithEqualsOperator Instance { get { return m_Instance; } }

        private Vector4ComparerWithEqualsOperator() {}
        /// <summary>
        /// Compares the actual and expected objects for equality using a custom comparison mechanism.
        /// </summary>
        /// <param name="expected">Expected Vector4 used to compare</param>
        /// <param name="actual">Actual Vector4 value to test.</param>
        /// <returns>Returns true if expected and actual objects are equal, otherwise it returns false.</returns>
        /// <example>
        /// <code>
        /// [TestFixture]
        /// public class Vector4Test
        /// {
        ///     [Test]
        ///     public void VerifyThat_TwoVector4ObjectsAreEqual()
        ///     {
        ///         var actual = new Vector4(10e-7f, 10e-7f, 10e-7f, 10e-7f);
        ///         var expected = new Vector4(0f, 0f, 0f, 0f);
        ///
        ///         Assert.That(actual, Is.EqualTo(expected).Using(Vector4ComparerWithEqualsOperator.Instance));
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool Equals(Vector4 expected, Vector4 actual)
        {
            return expected == actual;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <param name="vec4"> A not null Vector4 object</param>
        /// <returns>Returns 0</returns>
        public int GetHashCode(Vector4 vec4)
        {
            return 0;
        }
    }
}
