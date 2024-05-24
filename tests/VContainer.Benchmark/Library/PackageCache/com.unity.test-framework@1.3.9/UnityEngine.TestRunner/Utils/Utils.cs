using System;

namespace UnityEngine.TestTools.Utils
{
    /// <summary>
    /// This contains test utility functions for float value comparison and creating primitives.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Relative epsilon comparison of two float values for equality.
        /// The relative error is the absolute error divided by the magnitude of the exact value.
        /// </summary>
        /// <param name="expected">The expected float value used to compare.</param>
        /// <param name="actual">The actual float value to test.</param>
        /// <param name="epsilon"> Epsilon is the relative error to be used in relative epsilon comparison.</param>
        /// <returns>Returns true if the actual value is equivalent to the expected value.</returns>
        /// <example>
        /// <code>
        /// [TestFixture]
        /// class UtilsTests
        /// {
        ///     [Test]
        ///     public void CheckThat_FloatsAreEqual()
        ///     {
        ///         float expected = 10e-8f;
        ///         float actual = 0f;
        ///         float allowedRelativeError = 10e-6f;
        ///
        ///         Assert.That(Utils.AreFloatsEqual(expected, actual, allowedRelativeError), Is.True);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static bool AreFloatsEqual(float expected, float actual, float epsilon)
        {
            // special case for infinity
            if (expected == Mathf.Infinity || actual == Mathf.Infinity || expected == Mathf.NegativeInfinity || actual == Mathf.NegativeInfinity)
                return expected == actual;

            // we cover both relative and absolute tolerance with this check
            // which is better than just relative in case of small (in abs value) args
            // please note that "usually" approximation is used [i.e. abs(x)+abs(y)+1]
            // but we speak about test code so we dont care that much about performance
            // but we do care about checks being more precise
            return Math.Abs(actual - expected) <= epsilon * Mathf.Max(Mathf.Max(Mathf.Abs(actual), Mathf.Abs(expected)), 1.0f);
        }

        /// <summary>
        /// Compares two floating point numbers for equality under the given absolute tolerance.
        /// </summary>
        /// <param name="expected">The expected float value used to compare.</param>
        /// <param name="actual">The actual float value to test.</param>
        /// <param name="allowedAbsoluteError">AllowedAbsoluteError is the permitted error tolerance.</param>
        /// <returns> Returns true if the actual value is equivalent to the expected value under the given tolerance.
        /// </returns>
        /// <example>
        /// <code>
        /// [TestFixture]
        /// class UtilsTests
        /// {
        ///     [Test]
        ///     public void CheckThat_FloatsAreAbsoluteEqual()
        ///     {
        ///         float expected = 0f;
        ///         float actual = 10e-6f;
        ///         float error = 10e-5f;
        ///
        ///         Assert.That(Utils.AreFloatsEqualAbsoluteError(expected, actual, error), Is.True);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static bool AreFloatsEqualAbsoluteError(float expected, float actual, float allowedAbsoluteError)
        {
            return Math.Abs(actual - expected) <= allowedAbsoluteError;
        }

        /// <summary>
        /// Analogous to GameObject.CreatePrimitive, but creates a primitive mesh renderer with fast shader instead of a default builtin shader.
        /// Optimized for testing performance.
        /// </summary>
        /// <returns>A GameObject with primitive mesh renderer and collider.</returns>
        /// <param name="type">The type of primitive object to create.</param>
        public static GameObject CreatePrimitive(PrimitiveType type)
        {
            var prim = GameObject.CreatePrimitive(type);
            var renderer = prim.GetComponent<Renderer>();
            if (renderer)
                renderer.sharedMaterial = new Material(Shader.Find("VertexLit"));
            return prim;
        }
    }
}
