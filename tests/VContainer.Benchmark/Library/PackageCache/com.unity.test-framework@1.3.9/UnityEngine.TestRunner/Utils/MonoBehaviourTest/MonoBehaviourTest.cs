using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// This is a wrapper that allows running tests on MonoBehaviour scripts. Inherits from <see cref="CustomYieldInstruction"/>.
    /// </summary>
    /// <typeparam name="T">A MonoBehaviour component created for the test and attached to the tests [GameObject](https://docs.unity3d.com/ScriptReference/GameObject.html).</typeparam>
    public class MonoBehaviourTest<T> : CustomYieldInstruction where T : MonoBehaviour, IMonoBehaviourTest
    {
        /// <summary>A MonoBehaviour component created for the test and attached to the tests [GameObject](https://docs.unity3d.com/ScriptReference/GameObject.html).</summary>
        public T component { get; }
        /// <summary>
        /// A `GameObject` created as a container for the test component.
        /// </summary>
        public GameObject gameObject { get { return component.gameObject; } }
        /// <summary>
        /// `MonoBehaviourTest` is a [coroutine](https://docs.unity3d.com/ScriptReference/Coroutine.html) and a helper for writing MonoBehaviour tests.
        /// Yield a `MonoBehaviour`Test when using the `UnityTest` attribute to instantiate the `MonoBehaviour` you wish to test and wait for it to finish running. Implement the `IMonoBehaviourTest` interface on the `MonoBehaviour` to state when the test completes.
        /// </summary>
        /// <param name="dontDestroyOnLoad"></param>
        /// <example>
        /// <code>
        /// [UnityTest]
        /// public IEnumerator MonoBehaviourTest_Works()
        /// {
        ///     yield return new MonoBehaviourTest&lt;MyMonoBehaviourTest&gt;();
        /// }
        ///
        /// public class MyMonoBehaviourTest : MonoBehaviour, IMonoBehaviourTest
        /// {
        ///     private int frameCount;
        ///     public bool IsTestFinished
        ///     {
        ///         get { return frameCount &gt; 10; }
        ///     }
        ///
        ///     void Update()
        ///     {
        ///         frameCount++;
        ///     }
        /// }
        /// </code>
        /// </example>
        public MonoBehaviourTest(bool dontDestroyOnLoad = true)
        {
            var go = new GameObject("MonoBehaviourTest: " + typeof(T).FullName);
            component = go.AddComponent<T>();
            if (dontDestroyOnLoad)
            {
                Object.DontDestroyOnLoad(go);
            }
        }

        /// <summary>
        /// (Inherited) Returns `true`` if the test is not finished yet, which keeps the coroutine suspended
        /// </summary>
        public override bool keepWaiting
        {
            get { return !component.IsTestFinished; }
        }
    }
}
