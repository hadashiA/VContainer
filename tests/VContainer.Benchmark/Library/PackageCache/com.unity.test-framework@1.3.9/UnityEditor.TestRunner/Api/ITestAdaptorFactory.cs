using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine.TestRunner.TestLaunchers;

namespace UnityEditor.TestTools.TestRunner.Api
{
    internal interface ITestAdaptorFactory
    {
        ITestAdaptor Create(ITest test);
        ITestAdaptor Create(RemoteTestData testData);
        ITestResultAdaptor Create(ITestResult testResult);
        ITestResultAdaptor Create(RemoteTestResultData testResult, RemoteTestResultDataWithTestData allData);
        ITestAdaptor BuildTree(RemoteTestResultDataWithTestData data);
        IEnumerator<ITestAdaptor> BuildTreeAsync(RemoteTestResultDataWithTestData data);
        void ClearResultsCache();
        void ClearTestsCache();
    }
}
