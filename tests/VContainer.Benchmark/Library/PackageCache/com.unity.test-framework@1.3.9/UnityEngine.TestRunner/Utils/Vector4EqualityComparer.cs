using System;
using System.Collections.Generic;

namespace UnityEngine.TestTools.Utils
{
    /// <summary>
    /// Use this class to compare two Vector4 objects for equality with NUnit constraints. Call Vector4EqualityComparer.Instance to perform comparisons using default calculation error value 0.0001f. To set a custom test value, instantiate a new comparer using the one argument constructor.
    /// </summary>
    public class Vector4EqualityComparer : IEqualityComparer<Vector4>
    {
        private const float k_DefaultError = 0.0001f;
        private readonly float AllowedError;

        private static readonly Vector4EqualityComparer m_Instance = new Vector4EqualityComparer();
        /// <summary>
        /// A comparer instance with the default calculation error value set to 0.0001f.
        /// </summary>
        public static Vector4EqualityComparer Instance { get { return m_Instance; } }

        private Vector4EqualityComparer() : this(k_DefaultError) {}
        /// <summary>
        /// Initializes an instance of Vector4Equality comparer with custom allowed calculation error.
        /// </summary>
        /// <param name="allowedError">This value identifies the calculation error allowed.</param>
        public Vector4EqualityComparer(float allowedError)
        {
            AllowedError = allowedError;
        }

        /// <summary>
        /// Compares the actual and expected Vector4 objects for equality using <see cref="Utils.AreFloatsEqual"/> to compare the x, y, z, and w attributes of Vector4.
        /// </summary>
        /// <param name="expected">The expected Vector4 used for comparison</param>
        /// <param name="actual">The actual Vector4 to test</param>
        /// <returns>True if the vectors are equals, false otherwise.</returns>
        /// <example>
        /// <code>
        ///[TestFixture]
        /// public class Vector4Test
        /// {
        ///     [Test]
        ///     public void VerifyThat_TwoVector4ObjectsAreEqual()
        ///     {
        ///         // Custom error 10e-6f
        ///         var actual = new Vector4(0, 0, 1e-6f, 1e-6f);
        ///         var expected = new Vector4(1e-6f, 0f, 0f, 0f);
        ///         var comparer = new Vector4EqualityComparer(10e-6f);
        ///
        ///         Assert.That(actual, Is.EqualTo(expected).Using(comparer));
        ///
        ///         // Default error 0.0001f
        ///         actual = new Vector4(0.01f, 0.01f, 0f, 0f);
        ///         expected = new Vector4(0.01f, 0.01f, 0f, 0f);
        ///
        ///         Assert.That(actual, Is.EqualTo(expected).Using(Vector4EqualityComparer.Instance));
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool Equals(Vector4 expected, Vector4 actual)
        {
            return Utils.AreFloatsEqual(expected.x, actual.x, AllowedError) &&
                Utils.AreFloatsEqual(expected.y, actual.y, AllowedError) &&
                Utils.AreFloatsEqual(expected.z, actual.z, AllowedError) &&
                Utils.AreFloatsEqual(expected.w, actual.w, AllowedError);
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
