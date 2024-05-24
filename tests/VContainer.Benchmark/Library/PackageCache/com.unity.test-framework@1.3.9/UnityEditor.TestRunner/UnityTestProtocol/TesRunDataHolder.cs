using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.TestRunner.UnityTestProtocol
{
    [Obsolete("No longer in use")]
	public interface ITestRunDataHolder
    {
		IList<TestRunData> TestRunDataList { get; }
    }

    [Obsolete("No longer in use")]
    public class TestRunDataHolder: ScriptableSingleton<TestRunDataHolder>, ISerializationCallbackReceiver, ITestRunDataHolder
    {
        [SerializeField]
        private TestRunData[] TestRunData;
        public IList<TestRunData> TestRunDataList { get; private set; } = new List<TestRunData>();

        public void OnBeforeSerialize()
        {
            TestRunData = TestRunDataList.ToArray(); 
        }

        public void OnAfterDeserialize()
        {
            TestRunDataList = TestRunData.ToList();
        }
    }
}