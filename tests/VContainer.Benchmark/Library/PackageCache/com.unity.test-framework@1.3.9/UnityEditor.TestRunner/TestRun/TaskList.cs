using System;
using System.Collections.Generic;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Events;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Scene;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    internal static class TaskList
    {
        public static IEnumerable<TestTaskBase> GetTaskList(ExecutionSettings settings)
        {
            if (settings == null)
            {
                yield break;
            }

            if (settings.EditModeIncluded() || (PlayerSettings.runPlayModeTestAsEditModeTest && settings.PlayModeInEditorIncluded()))
            {
                yield return new SaveModifiedSceneTask();
                yield return new RegisterFilesForCleanupVerificationTask();
                yield return new SaveUndoIndexTask();
                yield return new BuildTestTreeTask(TestPlatform.EditMode);
                yield return new PrebuildSetupTask();
                yield return new RemoveAdditionalUntitledSceneTask();
                yield return new SaveSceneSetupTask();
                yield return new CreateNewSceneTask();
                yield return new CreateEventsTask();
                yield return new RegisterTestRunCallbackEventsTask();
                yield return new InitializeTestProgressTask();
                yield return new UpdateTestProgressTask();
                yield return new GenerateContextTask();
                yield return new EnableTestOutLoggerTask();
                yield return new SetupConstructDelegatorTask();
                yield return new RegisterCallbackDelegatorEventsTask();
                yield return new RunStartedInvocationEvent();
                yield return new EditModeRunTask();
                yield return new RunFinishedInvocationEvent();
                yield return new CleanupConstructDelegatorTask();
                yield return new PostbuildCleanupTask();
                yield return new CleanUpContext();
                yield return new RestoreSceneSetupTask();
                yield return new PerformUndoTask();
                yield return new CleanupVerificationTask();
                yield return new UnlockReloadAssembliesTask();
                yield break;
            }

            if (settings.PlayModeInEditorIncluded() && !PlayerSettings.runPlayModeTestAsEditModeTest)
            {
                yield return new GenerateContextTask();
                yield return new SaveModifiedSceneTask();
                yield return new LegacyPlayModeRunTask();
                yield return new CleanUpContext();
                yield return new UnlockReloadAssembliesTask();
                yield break;
            }

            if (settings.PlayerIncluded())
            {
                yield return new LegacyPlayerRunTask();
                yield return new UnlockReloadAssembliesTask();
            }
        }
    }
}
