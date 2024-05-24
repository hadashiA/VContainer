using System;
using System.Collections.Generic;

namespace UnityEngine.TestTools.Utils
{
    /// <summary>
    /// Use this class to compare two Color objects. ColorEqualityComparer.Instance has default calculation error value set to 0.01f. To set a test specific error value instantiate a comparer instance using the one argument constructor.
    /// </summary>
    public class ColorEqualityComparer : IEqualityComparer<Color>
    {
        private const float k_DefaultError = 0.01f;
        private readonly float AllowedError;


        private static readonly ColorEqualityComparer m_Instance = new ColorEqualityComparer();
        /// <summary>
        ///A singleton instance of the comparer with a default error value set to 0.01f.
        /// </summary>
        public static ColorEqualityComparer Instance { get { return m_Instance; } }

        private ColorEqualityComparer() : this(k_DefaultError)
        {
        }
        /// <summary>
        /// Creates an instance of the comparer with a custom error value.
        /// </summary>
        /// <param name="error">The custom error value.</param>
        public ColorEqualityComparer(float error)
        {
            AllowedError = error;
        }

        /// <summary>
        /// Compares the actual and expected Color objects for equality using <see cref="Utils.AreFloatsEqualAbsoluteError"/> to compare the RGB and Alpha attributes of Color. Returns true if expected and actual objects are equal otherwise, it returns false.
        /// </summary>
        /// <param name="expected">The expected Color value used to compare.</param>
        /// <param name="actual">The actual Color value to test.</param>
        /// <returns>True if actual and expected are equal, false otherwise</returns>
        /// <example>
        ///<code>
        /// [TestFixture]
        /// public class ColorEqualityTest
        /// {
        ///     [Test]
        ///     public void GivenColorsAreEqual_WithAllowedCalculationError()
        ///     {
        ///         // Using default error
        ///         var firstColor = new Color(0f, 0f, 0f, 0f);
        ///         var secondColor = new Color(0f, 0f, 0f, 0f);
        ///
        ///         Assert.That(firstColor, Is.EqualTo(secondColor).Using(ColorEqualityComparer.Instance));
        ///
        ///         // Allowed error 10e-5f
        ///         var comparer = new ColorEqualityComparer(10e-5f);
        ///         firstColor = new Color(0f, 0f, 0f, 1f);
        ///         secondColor = new Color(10e-6f, 0f, 0f, 1f);
        ///
        ///         Assert.That(firstColor, Is.EqualTo(secondColor).Using(comparer));
        ///     }
        /// }
        /// </code>
        /// </example>
        public bool Equals(Color expected, Color actual)
        {
            return Utils.AreFloatsEqualAbsoluteError(expected.r, actual.r, AllowedError) &&
                Utils.AreFloatsEqualAbsoluteError(expected.g, actual.g, AllowedError) &&
                Utils.AreFloatsEqualAbsoluteError(expected.b, actual.b, AllowedError) &&
                Utils.AreFloatsEqualAbsoluteError(expected.a, actual.a, AllowedError);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <param name="color">A not null Color object.</param>
        /// <returns>Returns 0.</returns>
        public int GetHashCode(Color color)
        {
            return 0;
        }
    }
}
