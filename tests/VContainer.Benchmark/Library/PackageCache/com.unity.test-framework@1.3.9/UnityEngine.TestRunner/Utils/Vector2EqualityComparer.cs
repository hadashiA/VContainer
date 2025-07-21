using System;
using System.Collections.Generic;

namespace UnityEngine.TestTools.Utils
{
    /// <summary>
    /// Use this class to compare two Vector2 objects for
    /// equality with NUnit constraints. Use the static
    /// <see cref="Vector2EqualityComparer.Instance"/>
    /// to have the calculation error value set to default 0.0001f.
    /// For any other error value, instantiate a new comparer
    /// object with the one argument constructor.
    /// </summary>
    public class Vector2EqualityComparer : IEqualityComparer<Vector2>
    {
        private const float k_DefaultError = 0.0001f;
        private readonly float AllowedError;

        private static readonly Vector2EqualityComparer m_Instance = new Vector2EqualityComparer();

        /// <summary>
        /// A comparer instance with the default error value set to 0.0001f.
        ///</summary>
        public static Vector2EqualityComparer Instance { get { return m_Instance; } }

        private Vector2EqualityComparer() : this(k_DefaultError)
        {
        }
        /// <summary>
        /// Initializes an instance of Vector2Equality comparer with custom allowed calculation error.
        /// </summary>
        /// <param name="error">This value identifies the calculation error allowed.</param>
        public Vector2EqualityComparer(float error)
        {
            AllowedError = error;
        }

        /// <summary>
        /// Compares the actual and expected Vector2 objects for equality using the <see cref="Utils.AreFloatsEqual"/> method.
        /// </summary>
        /// <param name="expected">The expected Vector2 used for comparison</param>
        /// <param name="actual">The actual Vector2 to test</param>
        /// <returns>True if the vectors are equals, false otherwise.</returns>
        /// <example>
        /// The following example shows how to verify if two Vector2 are equals
        ///<code>
        ///[TestFixture]
        /// public class Vector2Test
        /// {
        ///     [Test]
        ///     public void VerifyThat_TwoVector2ObjectsAreEqual()
        ///     {
        ///         // Custom calculation error
        ///         var actual = new Vector2(10e-7f, 10e-7f);
        ///         var expected = new Vector2(0f, 0f);
        ///         var comparer = new Vector2EqualityComparer(10e-6f);
        ///
        ///         Assert.That(actual, Is.EqualTo(expected).Using(comparer));
        ///
        ///         //Default error 0.0001f
        ///         actual = new Vector2(0.01f, 0.01f);
        ///         expected = new Vector2(0.01f, 0.01f);
        ///
        ///         Assert.That(actual, Is.EqualTo(expected).Using(Vector2EqualityComparer.Instance));
        ///      }
        ///  }
        /// </code>
        /// </example>
        public bool Equals(Vector2 expected, Vector2 actual)
        {
            return Utils.AreFloatsEqual(expected.x, actual.x, AllowedError) &&
                Utils.AreFloatsEqual(expected.y, actual.y, AllowedError);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <param name="vec2">A not null Vector2</param>
        /// <returns>Returns 0</returns>
        public int GetHashCode(Vector2 vec2)
        {
            return 0;
        }
    }
}
