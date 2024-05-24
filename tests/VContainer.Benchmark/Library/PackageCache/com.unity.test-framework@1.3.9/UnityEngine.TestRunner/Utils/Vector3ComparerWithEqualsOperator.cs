using System;
using System.Collections.Generic;

namespace UnityEngine.TestTools.Utils
{
    /// <summary>
    /// Use these classes to compare two objects of the same type for equality within the range of a given tolerance using NUnit or custom constraints . Call Instance to apply the default calculation error value to the comparison.
    /// </summary>
    public class Vector3ComparerWithEqualsOperator : IEqualityComparer<Vector3>
    {
        private static readonly Vector3ComparerWithEqualsOperator m_Instance = new Vector3ComparerWithEqualsOperator();
        /// <summary>
        /// A singleton instance of the comparer with a predefined default error value.
        /// </summary>
        public static Vector3ComparerWithEqualsOperator Instance { get { return m_Instance; } }

        private Vector3ComparerWithEqualsOperator() {}
        /// <summary>
        /// Compares the actual and expected objects for equality using a custom comparison mechanism.
        /// </summary>
        /// <param name="expected">Expected Vector3 used to compare</param>
        /// <param name="actual">Actual Vector3 value to test.</param>
        /// <returns>Returns true if expected and actual objects are equal, otherwise it returns false.</returns>
        /// <example>
        /// <code>
        /// [TestFixture]
        /// public class Vector3Test
        /// {
        ///     [Test]
        ///     public void VerifyThat_TwoVector3ObjectsAreEqual()
        ///     {
        ///         var actual = new Vector2(10e-7f, 10e-7f, 10e-7f);
        ///         var expected = new Vector2(0f, 0f, 0f);
        ///
        ///         Assert.That(actual, Is.EqualTo(expected).Using(Vector3ComparerWithEqualsOperator.Instance));
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool Equals(Vector3 expected, Vector3 actual)
        {
            return expected == actual;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <param name="vec3"> A not null Vector3 object</param>
        /// <returns>Returns 0</returns>
        public int GetHashCode(Vector3 vec3)
        {
            return 0;
        }
    }
}
