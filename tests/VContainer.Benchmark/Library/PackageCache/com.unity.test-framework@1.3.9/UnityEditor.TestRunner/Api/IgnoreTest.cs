using System;

namespace UnityEditor.TestTools.TestRunner.Api
{
    [Serializable]
    internal class IgnoreTest
    {
        public string test { get; set; }
        public string ignoreComment { get; set; }

        public UnityEngine.TestTools.IgnoreTest ParseToEngine()
        {
            return new UnityEngine.TestTools.IgnoreTest
            {
                test = test,
                ignoreComment = ignoreComment
            };
        }

        public override string ToString()
        {
            return $"'{test}': '{ignoreComment}'";
        }
    }

}
