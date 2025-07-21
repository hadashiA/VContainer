using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// PrebuildSetup attribute run if the test or test class is in the current test run. The test is included either by running all tests or setting a filter that includes the test. If multiple tests reference the same pre-built setup or post-build cleanup, then it only runs once.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public class PrebuildSetupAttribute : Attribute
    {
        /// <summary>
        ///  Initializes and returns an instance of PrebuildSetupAttribute by type.
        /// </summary>
        /// <param name="targetClass">The type of the target class.</param>
        public PrebuildSetupAttribute(Type targetClass)
        {
            TargetClass = targetClass;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="targetClassName"></param>
        /// <example>
        /// <code>
        /// [TestFixture]
        /// public class CreateSpriteTest : IPrebuildSetup
        /// {
        ///     Texture2D m_Texture;
        ///     Sprite m_Sprite;
        ///
        ///     public void Setup()
        ///     {
        ///
        ///         #if UNITY_EDITOR
        ///
        ///         var spritePath = "Assets/Resources/Circle.png";
        ///         var ti = UnityEditor.AssetImporter.GetAtPath(spritePath) as UnityEditor.TextureImporter;
        ///         ti.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
        ///         ti.SaveAndReimport();
        ///
        ///         #endif
        ///     }
        ///
        ///     [SetUp]
        ///     public void SetUpTest()
        ///     {
        ///         m_Texture = Resources.Load&lt;Texture2D&gt;("Circle");
        ///     }
        ///
        ///     [Test]
        ///     public void WhenNullTextureIsPassed_CreateShouldReturnNullSprite()
        ///     {
        ///
        ///         // Check with Valid Texture.
        ///         LogAssert.Expect(LogType.Log, "Circle Sprite Created");
        ///         Sprite.Create(m_Texture, new Rect(0, 0, m_Texture.width, m_Texture.height), new Vector2(0.5f, 0.5f));
        ///         Debug.Log("Circle Sprite Created");
        ///
        ///         // Check with NULL Texture. Should return NULL Sprite.
        ///         m_Sprite = Sprite.Create(null, new Rect(0, 0, m_Texture.width, m_Texture.heig`t), new Vector2(0.5f, 0.5f));
        ///         Assert.That(m_Sprite, Is.Null, "Sprite created with null texture should be null");
        ///     }
        /// }
        /// </code>
        /// Tip: Use `#if UNITY_EDITOR` if you want to access Editor only APIs, but the setup/cleanup is inside a **Play Mode** assembly.
        /// </example>
        public PrebuildSetupAttribute(string targetClassName)
        {
            TargetClass = AttributeHelper.GetTargetClassFromName(targetClassName, typeof(IPrebuildSetup));
        }

        internal Type TargetClass { get; private set; }
    }
}
