using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    internal class TestJobRunner : ITestJobRunner
    {
        internal ITestJobDataHolder testJobDataHolder = TestJobDataHolder.instance;

        internal Action<EditorApplication.CallbackFunction> SubscribeCallback =
            callback => EditorApplication.update += callback;

        // ReSharper disable once DelegateSubtraction
        internal Action<EditorApplication.CallbackFunction> UnsubscribeCallback =
            callback => EditorApplication.update -= callback;

        internal TestCommandPcHelper PcHelper = new EditModePcHelper();
        internal Func<ExecutionSettings, IEnumerable<TestTaskBase>> GetTasks = TaskList.GetTaskList;
        internal Action<Exception> LogException = Debug.LogException;
        internal Action<string> LogError = Debug.LogError;
        internal Action<string> ReportRunFailed = CallbacksDelegator.instance.RunFailed;
        internal Func<TestRunnerApi.RunProgressChangedEvent> RunProgressChanged = () => TestRunnerApi.runProgressChanged;

        private TestJobData m_JobData;
        private IEnumerator m_Enumerator;
        private string m_CurrentTaskName;

        public string RunJob(TestJobData data)
        {
            if (data == null)
            {
                throw new ArgumentException(null, nameof(data));
            }

            if (data.taskInfoStack == null)
            {
                throw new ArgumentException($"{nameof(data.taskInfoStack)} on {nameof(TestJobData)} is null.",
                    nameof(data));
            }

            if (IsRunningJob())
            {
                throw new Exception("TestJobRunner is already running a job.");
            }

            if (data.isHandledByRunner)
            {
                throw new Exception("Test job is already being handled.");
            }

            m_JobData = data;
            m_JobData.isHandledByRunner = true;

            if (!IsRunningJob())
            {
                m_JobData.isRunning = true;
                m_JobData.taskInfoStack.Push(new TaskInfo());
                testJobDataHolder.RegisterRun(this, m_JobData);
            }
            else // Is resuming run
            {
                var taskInfoBeforeResuming = m_JobData.taskInfoStack.Peek();
                if (taskInfoBeforeResuming.taskMode != TaskMode.Resume)
                {
                    m_JobData.taskInfoStack.Push(new TaskInfo
                    {
                        taskMode = TaskMode.Resume,
                        index = 0,
                        stopBeforeIndex = taskInfoBeforeResuming.index + (taskInfoBeforeResuming.pc > 0 ? 1 : 0)
                    });
                }
                else
                {
                    taskInfoBeforeResuming.index = 0;
                }
            }

            m_JobData.Tasks = GetTasks(data.executionSettings).ToArray();
            if (m_JobData.Tasks.Length == 0)
            {
                throw new Exception($"No tasks founds for {data.executionSettings}");
            }

            if (!data.executionSettings.runSynchronously)
            {
                SubscribeCallback(ExecuteCallback);
            }
            else
            {
                while (data.isRunning)
                {
                    ExecuteStep();
                }
            }

            return data.guid;
        }

        private void ExecuteCallback()
        {
            ExecuteStep();
            var c = 0;
            while (ShouldExecuteInstantly())
            {
                ExecuteStep();
                c++;

                if (c > 500)
                {
                    var taskInfo = m_JobData.taskInfoStack.Peek();
                    var taskName = taskInfo != null ? m_JobData.Tasks[taskInfo.index].GetType().Name : "null";
                    Debug.LogError(
                        $"Too many instant steps in test execution mode: {taskInfo?.taskMode}. Current task {taskName}.");
                    StopRun();
                    return;
                }
            }
        }

        private void ExecuteStep()
        {
            using (new ProfilerMarker(nameof(TestJobRunner) + "." + nameof(ExecuteStep)).Auto())
            {
                try
                {
                    if (m_JobData.taskInfoStack.Count == 0)
                    {
                        StopRun();
                        return;
                    }

                    var taskInfo = m_JobData.taskInfoStack.Peek();

                    if (m_Enumerator == null)
                    {
                        if (taskInfo.index >= m_JobData.Tasks.Length || (taskInfo.stopBeforeIndex > 0 &&
                                                                         taskInfo.index >= taskInfo.stopBeforeIndex))
                        {
                            m_JobData.taskInfoStack.Pop();
                            return;
                        }

                        var task = m_JobData.Tasks[taskInfo.index];
                        if (!task.ShouldExecute(taskInfo))
                        {
                            taskInfo.index++;
                            return;
                        }

                        m_JobData.runProgress.stepName = task.GetTitle();
                        m_CurrentTaskName = task.GetName();
                        using (new ProfilerMarker(m_CurrentTaskName + ".Setup").Auto())
                        {
                            m_Enumerator = task.Execute(m_JobData);
                        }

                        if (task.SupportsResumingEnumerator)
                        {
                            PcHelper.SetEnumeratorPC(m_Enumerator, taskInfo.pc);
                        }
                    }

                    using (new ProfilerMarker(m_CurrentTaskName + ".Progress").Auto())
                    {
                        var taskIsDone = !m_Enumerator.MoveNext();
                        if (!m_JobData.executionSettings.runSynchronously && taskInfo.taskMode == TaskMode.Normal)
                        {
                            if (taskIsDone)
                            {
                                m_JobData.runProgress.progress += RunProgress.progressPrTask;
                            }
                            ReportRunProgress(false);
                        }

                        if (taskIsDone)
                        {
                            taskInfo.index++;
                            taskInfo.pc = 0;
                            m_Enumerator = null;

                            return;
                        }
                    }

                    if (m_JobData.Tasks[taskInfo.index].SupportsResumingEnumerator)
                    {
                        taskInfo.pc = PcHelper.GetEnumeratorPC(m_Enumerator);
                    }
                }
                catch (TestRunCanceledException)
                {
                    StopRun();
                }
                catch (AggregateException ex)
                {
                    MarkJobAsError();
                    LogError(ex.Message);
                    foreach (var innerException in ex.InnerExceptions)
                    {
                        LogException(innerException);
                    }

                    ReportRunFailed("Multiple unexpected errors happened while running tests.");
                }
                catch (Exception ex)
                {
                    MarkJobAsError();
                    LogException(ex);
                    ReportRunFailed("An unexpected error happened while running tests.");
                }
            }
        }

        public bool CancelRun()
        {
            if (m_JobData == null || m_JobData.taskInfoStack.Count == 0 ||
                m_JobData.taskInfoStack.Peek().taskMode == TaskMode.Canceled)
            {
                return false;
            }

            var lastIndex = m_JobData.taskInfoStack.Last().index;
            m_JobData.taskInfoStack.Clear();
            m_JobData.taskInfoStack.Push(new TaskInfo
            {
                index = lastIndex,
                taskMode = TaskMode.Canceled
            });
            m_Enumerator = null;
            return true;
        }

        private bool ShouldExecuteInstantly()
        {
            if (m_JobData.taskInfoStack.Count == 0)
            {
                return false;
            }

            var taskInfo = m_JobData.taskInfoStack.Peek();
            var canRunInstantly =
                m_JobData.Tasks.Length <= taskInfo.index || m_JobData.Tasks[taskInfo.index].CanRunInstantly;
            return taskInfo.taskMode != TaskMode.Normal && taskInfo.taskMode != TaskMode.Canceled && canRunInstantly;
        }

        public bool IsRunningJob()
        {
            return m_JobData != null && m_JobData.taskInfoStack != null && m_JobData.taskInfoStack.Count > 0;
        }

        public TestJobData GetData()
        {
            return m_JobData;
        }

        private void StopRun()
        {
            m_JobData.isRunning = false;
            UnsubscribeCallback(ExecuteCallback);
            testJobDataHolder.UnregisterRun(this, m_JobData);

            foreach (var task in m_JobData.Tasks)
            {
                if (task is IDisposable disposableTask)
                {
                    try
                    {
                        disposableTask.Dispose();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            if (!m_JobData.executionSettings.runSynchronously)
            {
                ReportRunProgress(true);
            }
        }

        private void ReportRunProgress(bool runHasFinished)
        {
            RunProgressChanged().Invoke(new TestRunProgress
            {
                CurrentStageName = m_JobData.runProgress.stageName ?? "",
                CurrentStepName = m_JobData.runProgress.stepName ?? "",
                Progress = m_JobData.runProgress.progress,
                ExecutionSettings = m_JobData.executionSettings,
                RunGuid = m_JobData.guid ?? "",
                HasFinished = runHasFinished,
            });
        }

        private void MarkJobAsError()
        {
            var currentTaskInfo = m_JobData.taskInfoStack.Peek();
            currentTaskInfo.taskMode = TaskMode.Error;
            currentTaskInfo.index++;
            currentTaskInfo.pc = 0;
            m_Enumerator = null;
        }
    }
}
