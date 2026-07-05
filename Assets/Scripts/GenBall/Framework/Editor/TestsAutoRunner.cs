using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace GenBall.Framework.Editor
{
    [InitializeOnLoad]
    public static class TestsAutoRunner
    {
        // ── Old trigger path (run_editmode_tests.sh, backward compat) ──
        private static readonly string TriggerFile = "Temp/.run_tests.trigger";
        private static readonly string DoneFile = "Temp/.run_tests.done";
        private static readonly string ResultsFile = "Temp/TestResults.txt";

        // ── New devtool trigger path (devtool.sh) ─────────────────────
        private static readonly string DevToolTriggerFile =
            "Temp/.devtool_test.trigger";
        private static readonly string DevToolDoneFile =
            "Temp/.devtool_test.done";
        private static readonly string DevToolResultsFile =
            "Temp/.devtool_test_result.json";

        private static TestRunnerApi s_Api;
        private static bool s_IsRunning;
        private static TestRunRequest s_CurrentRequest;

        /// <summary>Which trigger source is the current run from?</summary>
        private static bool s_DevToolMode;

        static TestsAutoRunner()
        {
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (s_IsRunning) return;

            string triggerPath = null;
            bool devToolMode = false;

            // Check devtool trigger first, then legacy trigger.
            if (File.Exists(DevToolTriggerFile))
            {
                triggerPath = DevToolTriggerFile;
                devToolMode = true;
            }
            else if (File.Exists(TriggerFile))
            {
                triggerPath = TriggerFile;
                devToolMode = false;
            }

            if (triggerPath == null) return;

            try
            {
                var json = File.ReadAllText(triggerPath);
                s_CurrentRequest = JsonUtility.FromJson<TestRunRequest>(json);
                s_IsRunning = true;
                s_DevToolMode = devToolMode;

                if (s_Api == null)
                {
                    s_Api = ScriptableObject.CreateInstance<TestRunnerApi>();
                }

                var filter = new Filter
                {
                    testMode = s_CurrentRequest.testMode == "PlayMode"
                        ? TestMode.PlayMode
                        : TestMode.EditMode
                };

                if (!string.IsNullOrEmpty(s_CurrentRequest.testAssembly))
                {
                    filter.assemblyNames =
                        new[] { s_CurrentRequest.testAssembly };
                }

                // groupNames uses FullNameFilter { IsRegex = true }.
                // Plain class names like "SimpleFsmTests" are valid
                // regex patterns (matching any test whose full name
                // contains that class name), but NUnit may anchor them.
                // Wrap in .* to guarantee substring match.
                if (!string.IsNullOrEmpty(s_CurrentRequest.testMethod))
                {
                    filter.testNames =
                        new[] { s_CurrentRequest.testMethod };
                }
                else if (!string.IsNullOrEmpty(s_CurrentRequest.testClass))
                {
                    filter.groupNames =
                        new[] { ".*" + s_CurrentRequest.testClass + ".*" };
                }
                else if (!string.IsNullOrEmpty(s_CurrentRequest.testNamespace))
                {
                    filter.groupNames =
                        new[] { ".*" + s_CurrentRequest.testNamespace + ".*" };
                }

                var callbacks = new TestCallbacks(s_CurrentRequest);
                s_Api.RegisterCallbacks(callbacks);
                s_Api.Execute(new ExecutionSettings(filter));
            }
            catch (Exception e)
            {
                WriteError($"Failed to start tests: {e.Message}\n{e.StackTrace}");
                Cleanup();
            }
        }

        private static void Cleanup()
        {
            s_IsRunning = false;
            s_CurrentRequest = null;
            try
            {
                File.Delete(s_DevToolMode ? DevToolTriggerFile : TriggerFile);
            }
            catch { }
        }

        private static void WriteError(string message)
        {
            try
            {
                var result = new TestResultCollection
                {
                    summary = new TestSummary
                    {
                        status = "Error",
                        totalTests = 0,
                        passedTests = 0,
                        failedTests = 0,
                        skippedTests = 0,
                        error = message
                    }
                };
                File.WriteAllText(
                    s_DevToolMode ? DevToolResultsFile : ResultsFile,
                    JsonUtility.ToJson(result, true));
                File.WriteAllText(
                    s_DevToolMode ? DevToolDoneFile : DoneFile, "");
            }
            catch { }
        }

        private class TestCallbacks : ICallbacks
        {
            private readonly TestRunRequest _request;
            private readonly List<TestResultItem> _results = new List<TestResultItem>();
            private DateTime _startTime;

            public TestCallbacks(TestRunRequest request)
            {
                _request = request;
                _startTime = DateTime.Now;
            }

            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                var duration = DateTime.Now - _startTime;

                var collection = new TestResultCollection
                {
                    summary = new TestSummary
                    {
                        status = result.FailCount > 0 ? "Failed" : "Passed",
                        totalTests = result.PassCount + result.FailCount + result.SkipCount + result.InconclusiveCount,
                        passedTests = result.PassCount,
                        failedTests = result.FailCount,
                        skippedTests = result.SkipCount,
                        duration = $"{duration.TotalSeconds:F2}s"
                    },
                    results = _results.ToArray()
                };

                try
                {
                    File.WriteAllText(
                        s_DevToolMode ? DevToolResultsFile : ResultsFile,
                        JsonUtility.ToJson(collection, true));
                    File.WriteAllText(
                        s_DevToolMode ? DevToolDoneFile : DoneFile, "");
                }
                catch { }
                finally
                {
                    Cleanup();
                }
            }

            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result)
            {
                var item = new TestResultItem
                {
                    name = result.FullName ?? result.Name,
                    status = result.ResultState,
                    duration = $"{result.Duration:F2}s",
                    message = result.Message,
                    stackTrace = result.StackTrace
                };

                if (_request.includePassingTests || result.ResultState != "Passed")
                {
                    if (!_request.includeMessages) item.message = null;
                    if (!_request.includeStacktrace) item.stackTrace = null;
                    _results.Add(item);
                }
            }
        }
    }

    [Serializable]
    public class TestRunRequest
    {
        public string testMode = "EditMode";
        public string testAssembly;
        public string testNamespace;
        public string testClass;
        public string testMethod;
        public bool includePassingTests;
        public bool includeMessages = true;
        public bool includeStacktrace;
    }

    [Serializable]
    public class TestResultCollection
    {
        public TestSummary summary;
        public TestResultItem[] results;
    }

    [Serializable]
    public class TestSummary
    {
        public string status;
        public int totalTests;
        public int passedTests;
        public int failedTests;
        public int skippedTests;
        public string duration;
        public string error;
    }

    [Serializable]
    public class TestResultItem
    {
        public string name;
        public string status;
        public string duration;
        public string message;
        public string stackTrace;
    }
}
