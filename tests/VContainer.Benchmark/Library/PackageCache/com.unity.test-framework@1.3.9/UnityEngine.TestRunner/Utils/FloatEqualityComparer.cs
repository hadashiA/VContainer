using System;
using System.Collections.Generic;

namespace UnityEngine.TestTools.Utils
{
    /// <summary>
    /// Use this class to compare two float values for equality with NUnit constraints. Use FloatEqualityComparer.Instance comparer to have the default error value set to 0.0001f. For any other error, use the one argument constructor to create a comparer.
    /// </summary>
    public class FloatEqualityComparer : IEqualityComparer<float>
    {
        private const float k_DefaultError = 0.0001f;
        private readonly float AllowedError;

        private static readonly  FloatEqualityComparer m_Instance = new FloatEqualityComparer();
        /// <summary>
        ///A singleton instance of the comparer with a default error value set to 0.0001f.
        /// </summary>
        public static FloatEqualityComparer Instance { get { return m_Instance; } }

        private FloatEqualityComparer() : this(k_DefaultError) {}

        /// <summary>
        /// Initializes an instance of a FloatEqualityComparer with a custom error value instead of the default 0.0001f.
        /// </summary>
        /// <param name="allowedError">The custom error value</param>
        public FloatEqualityComparer(float allowedError)
        {
            AllowedError = allowedError;
        }

        /// <summary>
        /// Compares the actual and expected float values for equality using <see cref="Utils.AreFloatsEqual"/>.
        /// </summary>
        /// <param name="expected">The expected float value used to compare.</param>
        /// <param name="actual">The actual float value to test.</param>
        /// <returns>True if the values are equals, false otherwise.</returns>
        /// <example>
        /// <code>
        /// [TestFixture]
        /// public class FloatsTest
        ///{
        ///    [Test]
        ///    public void VerifyThat_TwoFloatsAreEqual()
        ///    {
        ///        var comparer = new FloatEqualityComparer(10e-6f);
        ///        var actual = -0.00009f;
        ///        var expected = 0.00009f;
        ///
        ///        Assert.That(actual, Is.EqualTo(expected).Using(comparer));
        ///
        ///        // Default relative error 0.0001f
        ///        actual = 10e-8f;
        ///        expected = 0f;
        ///
        ///        Assert.That(actual, Is.EqualTo(expected).Using(FloatEqualityComparer.Instance));
        ///    }
        ///}
        /// </code>
        /// </example>
        public bool Equals(float expected, float actual)
        {
            return Utils.AreFloatsEqual(expected, actual, AllowedError);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <param name="value">A not null float number.</param>
        /// <returns>Returns 0.</returns>
        public int GetHashCode(float value)
        {
            return 0;
        }
    }
}
