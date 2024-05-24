using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.NUnitExtensions;
using UnityEngine.TestTools.Utils;

namespace UnityEngine.TestTools.TestRunner
{
    [Serializable]
    [AddComponentMenu("")]
    internal class PlaymodeTestsController : MonoBehaviour
    {
        private IEnumerator m_TestSteps;

        public static PlaymodeTestsController ActiveController { get; private set; }

        [SerializeField]
        private List<string> m_AssembliesWithTests;
        public List<string> AssembliesWithTests
        {
            get
            {
                return m_AssembliesWithTests;
            }
            set
            {
                m_AssembliesWithTests = value;
            }
        }

        [SerializeField]
        internal TestStartedEvent testStartedEvent = new TestStartedEvent();
        [SerializeField]
        internal TestFinishedEvent testFinishedEvent = new TestFinishedEvent();
        [SerializeField]
        internal RunStartedEvent runStartedEvent = new RunStartedEvent();
        [SerializeField]
        internal RunFinishedEvent runFinishedEvent = new RunFinishedEvent();

        //DO NOT change this string, third party code is using this string to identify the test runner game object.
        internal const string kPlaymodeTestControllerName = "Code-based tests runner";

        [SerializeField]
        public PlaymodeTestsControllerSettings settings = new PlaymodeTestsControllerSettings();

        internal UnityTestAssemblyRunner m_Runner;

        public IEnumerator Start()
        {
            UnityTestExecutionContext.CurrentContext = new UnityTestExecutionContext()
            {
                FeatureFlags = settings.featureFlags,
                RetryCount = settings.retryCount,
                RepeatCount = settings.repeatCount,
                Automated = settings.automated
            };
            ActiveController = this;
            //Skip 2 frame because Unity.
            yield return null;
            yield return null;
            StartCoroutine(Run());
        }

        internal static bool IsControllerOnScene()
        {
            return GameObject.Find(kPlaymodeTestControllerName) != null;
        }

        internal static PlaymodeTestsController GetController()
        {
            return GameObject.Find(kPlaymodeTestControllerName).GetComponent<PlaymodeTestsController>();
        }

        public IEnumerator TestRunnerCoroutine()
        {
            while (m_TestSteps.MoveNext())
            {
                yield return m_TestSteps.Current;
            }

            if (m_Runner.IsTestComplete)
            {
                runFinishedEvent.Invoke(m_Runner.Result);

                yield return null;
            }
        }

        public IEnumerator Run()
        {
            CoroutineTestWorkItem.monoBehaviourCoroutineRunner = this;
            gameObject.hideFlags |= HideFlags.DontSave;

            if (settings.sceneBased)
            {
                SceneManager.LoadScene(1, LoadSceneMode.Additive);
                yield return null;
            }

            var testListUtil = new PlayerTestAssemblyProvider(new AssemblyLoadProxy(), m_AssembliesWithTests);
            m_Runner = new UnityTestAssemblyRunner(new UnityTestAssemblyBuilder(settings.orderedTestNames, settings.randomOrderSeed), new PlaymodeWorkItemFactory(), UnityTestExecutionContext.CurrentContext);

            var loadedTests = m_Runner.Load(testListUtil.GetUserAssemblies().Select(a => a.Assembly).ToArray(), TestPlatform.PlayMode, UnityTestAssemblyBuilder.GetNUnitTestBuilderSettings(TestPlatform.PlayMode));
            loadedTests.ParseForNameDuplicates();
            runStartedEvent.Invoke(m_Runner.LoadedTest);

            var testListenerWrapper = new TestListenerWrapper(testStartedEvent, testFinishedEvent);
            m_TestSteps = m_Runner.Run(testListenerWrapper, settings.BuildNUnitFilter()).GetEnumerator();

            yield return TestRunnerCoroutine();
        }

        public void Cleanup()
        {
            if (m_Runner != null)
            {
                m_Runner.StopRun();
                m_Runner = null;
            }
            if (Application.isEditor)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        public static void TryCleanup()
        {
            var controller = GetController();
            if (controller != null)
            {
                controller.Cleanup();
            }
        }
    }
}
