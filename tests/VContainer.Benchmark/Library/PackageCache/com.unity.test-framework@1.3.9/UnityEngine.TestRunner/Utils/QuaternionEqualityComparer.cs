using System;
using System.Collections.Generic;

namespace UnityEngine.TestTools.Utils
{
    /// <summary>
    /// Use this utility to compare two Quaternion objects for equality
    /// with NUnit assertion constraints.
    /// Use the static instance QuaternionEqualityComparer.Instance
    /// to have the default calculation error value set to 0.00001f.
    /// For any other custom error value, use the one argument constructor.
    /// </summary>
    public class QuaternionEqualityComparer : IEqualityComparer<Quaternion>
    {
        private const float k_DefaultError = 0.00001f;
        private readonly float AllowedError;

        private static readonly QuaternionEqualityComparer m_Instance = new QuaternionEqualityComparer();
        /// <summary>
        ///A comparer instance with the default error value 0.00001f.
        /// </summary>
        public static QuaternionEqualityComparer Instance { get { return m_Instance; } }


        private QuaternionEqualityComparer() : this(k_DefaultError) {}
        /// <summary>
        /// Creates an instance of the comparer with a custom allowed error value.
        /// </summary>
        /// <param name="allowedError">Describes the custom allowed error value</param>
        public QuaternionEqualityComparer(float allowedError)
        {
            AllowedError = allowedError;
        }

        /// <summary>
        /// Compares the actual and expected Quaternion objects
        /// for equality using the <see cref="Quaternion.Dot "/> method.
        /// </summary>
        /// <param name="expected">Expected Quaternion value used for comparison</param>
        /// <param name="actual">Actual Quaternion value to test</param>
        /// <returns>True if the quaternion are equals, false otherwise.</returns>
        /// <example>
        /// The following example shows how to verify if two Quaternion are equals
        /// <code>
        /// [TestFixture]
        /// public class QuaternionTest
        /// {
        ///     [Test]
        ///     public void VerifyThat_TwoQuaternionsAreEqual()
        ///     {
        ///         var actual = new Quaternion(10f, 0f, 0f, 0f);
        ///         var expected = new Quaternion(1f, 10f, 0f, 0f);
        ///         var comparer = new QuaternionEqualityComparer(10e-6f);
        ///
        ///         Assert.That(actual, Is.EqualTo(expected).Using(comparer));
        ///
        ///         //Using default error 0.00001f
        ///         actual = new Quaternion(10f, 0f, 0.1f, 0f);
        ///         expected = new Quaternion(1f, 10f, 0.1f, 0f);
        ///
        ///         Assert.That(actual, Is.EqualTo(expected).Using(QuaternionEqualityComparer.Instance));
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool Equals(Quaternion expected, Quaternion actual)
        {
            return Mathf.Abs(Quaternion.Dot(expected, actual)) > (1.0f - AllowedError);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <param name="quaternion">A not null Quaternion</param>
        /// <returns>Returns 0</returns>
        public int GetHashCode(Quaternion quaternion)
        {
            return 0;
        }
    }
}
