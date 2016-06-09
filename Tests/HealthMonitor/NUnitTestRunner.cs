using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NUnit.Core;
using HealthMonitor.Configuration;
using Fr8.Testing.Integration;

namespace HealthMonitor
{
    public class NUnitTestRunner
    {
        string _appInsightsInstrumentationKey;

        public NUnitTestRunner()
        {
        }

        public NUnitTestRunner(string appInsightsInstrumentationKey)
        {
            _appInsightsInstrumentationKey = appInsightsInstrumentationKey;
        }
        
        public class NUnitTestRunnerFilter : ITestFilter
        {
            public bool IsEmpty { get { return false; } }

            public bool Match(ITest test)
            {
                return test.RunState != RunState.NotRunnable;
            }

            public bool Pass(ITest test)
            {
                return test.RunState != RunState.NotRunnable;
            }
        }


        private Type[] GetTestSuiteTypes(bool skipLocal)
        {
            var healthMonitorCS = (HealthMonitorConfigurationSection)
                ConfigurationManager.GetSection("healthMonitor");

            if (healthMonitorCS == null || healthMonitorCS.TestSuites == null)
            {
                return null;
            }

            var testSuites = healthMonitorCS.TestSuites
                .Select(x => Type.GetType(x.Type))
                .Where(x => x != null);

            if (skipLocal)
            {
                testSuites = testSuites
                    .Where(x => !x.GetCustomAttributes(typeof(SkipLocalAttribute), false).Any());
            }

            return testSuites.ToArray();
        }

        public TestReport Run(string specificTestName = null, bool skipLocal = false)
        {
            var testSuiteTypes = GetTestSuiteTypes(skipLocal);
            if (testSuiteTypes == null || testSuiteTypes.Length == 0)
            {
                return new TestReport()
                {
                    Tests = new List<TestReportItem>()
                };
            }

            var testPackage = new TestPackage("HelthMonitor test package");
            var testSuite = new TestSuite("HelthMonitor test suite");

            TestExecutionContext.CurrentContext.TestPackage = testPackage;

            if (string.IsNullOrEmpty(specificTestName))
            {
                foreach (var testSuiteType in testSuiteTypes)
                {
                    var test = TestFixtureBuilder.BuildFrom(testSuiteType);
                    testSuite.Tests.Add(test);
                }
            }
            else
            {
                Test specificTest = null;
                string specificTestMethod = null;

                if (specificTestName.Contains("#"))
                {
                    var tokens = specificTestName.Split('#');

                    specificTestName = tokens[0];
                    specificTestMethod = tokens[1];
                }

                var specificTestType = testSuiteTypes.FirstOrDefault(x => x.FullName == specificTestName);
                if (specificTestType != null)
                {
                    specificTest = TestFixtureBuilder.BuildFrom(specificTestType);
                }

                if (specificTest != null && !string.IsNullOrEmpty(specificTestMethod))
                {
                    var testsToRemove = new List<Test>();
                    foreach (Test test in specificTest.Tests)
                    {
                        var testMethod = test as TestMethod;
                        if (testMethod != null)
                        {
                            if (testMethod.Method.Name != specificTestMethod)
                            {
                                testsToRemove.Add(test);
                            }
                        }
                    }

                    foreach (var test in testsToRemove)
                    {
                        specificTest.Tests.Remove(test);
                    }
                }

                if (specificTest != null)
                {
                    testSuite.Tests.Add(specificTest);
                }
            }
            
            using (NUnitTraceListener listener = new NUnitTraceListener(_appInsightsInstrumentationKey))
            {
                var testResult = testSuite.Run(listener, new NUnitTestRunnerFilter());
               
                var testReport = GenerateTestReport(testResult);
                return testReport;
            }
        }

        private TestReport GenerateTestReport(TestResult testResult)
        {
            var reportItems = VisitTestResult(testResult);

            var testReport = new TestReport()
            {
                Tests = reportItems
            };

            return testReport;
        }

        private IEnumerable<TestReportItem> VisitTestResult(TestResult testResult)
        {
            var result = new List<TestReportItem>();

            if (testResult.Test.TestType == "TestMethod")
            {
                var reportItem = new TestReportItem()
                {
                    Name = testResult.FullName,
                    Success = testResult.IsSuccess || (!testResult.IsError && !testResult.IsFailure)
                };

                if (!testResult.IsSuccess)
                {
                    reportItem.Message = testResult.Message;
                    reportItem.StackTrace = testResult.StackTrace;
                }

                result.Add(reportItem);
            }
            else
            {
                if (testResult.Results != null)
                {
                    foreach (TestResult childTestResult in testResult.Results)
                    {
                        result.AddRange(VisitTestResult(childTestResult));
                    }
                }
            }

            return result;
        }
    }
}
