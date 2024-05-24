using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestTools.Logging;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal abstract class BuildActionTaskBase<T> : TestTaskBase
    {
        private string typeName;
        internal IAttributeFinder attributeFinder;
        internal RuntimePlatform targetPlatform = Application.platform;
        internal Action<string> logAction = Debug.Log;
        internal Func<ILogScope> logScopeProvider = () => new LogScope();
        internal Func<Type, object> createInstance = Activator.CreateInstance;

        protected BuildActionTaskBase(IAttributeFinder attributeFinder)
        {
            this.attributeFinder = attributeFinder;
            typeName = typeof(T).Name;
        }

        protected abstract void Action(T target);

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.testTree == null)
            {
                throw new Exception($"Test tree is not available for {GetType().Name}.");
            }

            var enumerator = ExecuteMethods(testJobData.testTree, testJobData.executionSettings.BuildNUnitFilter());
            while (enumerator.MoveNext())
            {
                yield return null;
            }
        }
        
        protected IEnumerator ExecuteMethods(ITest testTree, ITestFilter testRunnerFilter)
        {
            var exceptions = new List<Exception>();

            foreach (var targetClassType in attributeFinder.Search(testTree, testRunnerFilter, targetPlatform))
            {
                try
                {
                    var targetClass = (T)createInstance(targetClassType);

                    logAction($"Executing {typeName} for: {targetClassType.FullName}.");

                    using (var logScope = logScopeProvider())
                    {
                        Action(targetClass);
                        logScope.EvaluateLogScope(true);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                yield return null;
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException($"One or more exceptions when executing {typeName}.", exceptions);
            }
        }
    }
}
