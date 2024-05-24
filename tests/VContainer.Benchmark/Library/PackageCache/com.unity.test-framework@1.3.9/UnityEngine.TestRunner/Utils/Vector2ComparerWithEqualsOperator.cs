using System;
using System.Collections.Generic;

namespace UnityEngine.TestTools.Utils
{
    /// <summary>
    /// Use these classes to compare two objects of the same type for equality within the range of a given tolerance using NUnit or custom constraints . Call Instance to apply the default calculation error value to the comparison.
    /// </summary>
    public class Vector2ComparerWithEqualsOperator : IEqualityComparer<Vector2>
    {
        private static readonly Vector2ComparerWithEqualsOperator m_Instance = new Vector2ComparerWithEqualsOperator();
        /// <summary>
        /// A singleton instance of the comparer with a predefined default error value.
        /// </summary>
        public static Vector2ComparerWithEqualsOperator Instance { get { return m_Instance; } }

        private Vector2ComparerWithEqualsOperator() {}
        /// <summary>
        /// Compares the actual and expected objects for equality using a custom comparison mechanism.
        /// </summary>
        /// <param name="expected">Expected Vector2 used to compare</param>
        /// <param name="actual">Actual Vector2 value to test.</param>
        /// <returns>Returns true if expected and actual objects are equal, otherwise it returns false.</returns>
        /// <example>
        /// <code>
        /// [TestFixture]
        /// public class Vector2Test
        /// {
        ///     [Test]
        ///     public void VerifyThat_TwoVector2ObjectsAreEqual()
        ///     {
        ///         var actual = new Vector2(10e-7f, 10e-7f);
        ///         var expected = new Vector2(0f, 0f);
        ///
        ///         Assert.That(actual, Is.EqualTo(expected).Using(Vector2ComparerWithEqualsOperator.Instance));
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool Equals(Vector2 expected, Vector2 actual)
        {
            return expected == actual;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <param name="vec2"> A not null Vector2 object</param>
        /// <returns>Returns 0</returns>
        public int GetHashCode(Vector2 vec2)
        {
            return 0;
        }
    }
}
