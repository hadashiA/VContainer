using System;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    internal class RequiredTestRunDataMissingException : Exception
    {
        public RequiredTestRunDataMissingException(string fieldName) : base($"The test run data '{fieldName}' is required and could not be found.")
        {
            FieldName = fieldName;
        }

        public string FieldName;
    }
}
