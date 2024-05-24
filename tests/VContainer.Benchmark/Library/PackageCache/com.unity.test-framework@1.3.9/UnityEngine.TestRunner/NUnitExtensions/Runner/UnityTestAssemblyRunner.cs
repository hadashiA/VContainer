using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    internal interface IUnityTestAssemblyRunner
    {
        ITest LoadedTest { get; }
        ITestResult Result { get; }
        bool IsTestLoaded { get; }
        bool IsTestRunning { get; }
        bool IsTestComplete { get; }
        UnityWorkItem TopLevelWorkItem { get; set; }
        UnityTestExecutionContext GetCurrentContext();
        ITest Load(Assembly[] assemblies, TestPlatform testPlatform, IDictionary<string, object> settings);
        void LoadTestTree(ITest testTree);
        IEnumerable Run(ITestListener listener, ITestFilter filter);
        void StopRun();
    }

    internal class UnityTestAssemblyRunner : IUnityTestAssemblyRunner
    {
        private readonly UnityTestAssemblyBuilder unityBuilder;
        private readonly WorkItemFactory m_Factory;

        protected UnityTestExecutionContext Context { get; set; }

        public UnityTestExecutionContext GetCurrentContext()
        {
            return UnityTestExecutionContext.CurrentContext;
        }

        protected IDictionary<string, object> Settings { get; set; }
        public ITest LoadedTest { get; protected set; }

        public ITestResult Result
        {
            get { return TopLevelWorkItem == null ? null : TopLevelWorkItem.Result; }
        }

        public bool IsTestLoaded
        {
            get { return LoadedTest != null; }
        }

        public bool IsTestRunning
        {
            get { return TopLevelWorkItem != null && TopLevelWorkItem.State == WorkItemState.Running; }
        }
        public bool IsTestComplete
        {
            get { return TopLevelWorkItem != null && TopLevelWorkItem.State == WorkItemState.Complete; }
        }

        public UnityTestAssemblyRunner(UnityTestAssemblyBuilder builder, WorkItemFactory factory, UnityTestExecutionContext context)
        {
            unityBuilder = builder;
            m_Factory = factory;
            Context = context;
        }

        public ITest Load(Assembly[] assemblies, TestPlatform testPlatform, IDictionary<string, object> settings)
        {
            // The setting of the TestMode should happen when running tests.
            if (Context != null)
            {
                Context.TestMode = testPlatform;
            }
            Settings = settings;

            if (settings.ContainsKey(FrameworkPackageSettings.RandomSeed))
                Randomizer.InitialSeed = (int)settings[FrameworkPackageSettings.RandomSeed];

            var tree = unityBuilder.Build(assemblies, Enumerable.Repeat(testPlatform, assemblies.Length).ToArray(), settings);

            return LoadedTest = tree;
        }

        public void LoadTestTree(ITest testTree)
        {
            LoadedTest = testTree;
        }

        public IEnumerable Run(ITestListener listener, ITestFilter filter)
        {
            TopLevelWorkItem = m_Factory.Create(LoadedTest, filter);
            TopLevelWorkItem.InitializeContext(Context);
            Context.Listener = listener;

            return TopLevelWorkItem.Execute();
        }

        public UnityWorkItem TopLevelWorkItem { get; set; }

        public void StopRun()
        {
            if (IsTestRunning)
            {
                TopLevelWorkItem.Cancel(false);
            }
        }
    }
}
