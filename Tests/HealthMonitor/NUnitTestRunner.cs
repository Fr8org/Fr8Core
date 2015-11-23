using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NUnit.Core;
using HealthMonitor.Configuration;

namespace HealthMonitor
{
    public class NUnitTestRunner
    {
        private Type[] GetTestSuiteTypes()
        {
            var healthMonitorCS = (HealthMonitorConfigurationSection)
                ConfigurationManager.GetSection("healthMonitor");

            if (healthMonitorCS == null || healthMonitorCS.TestSuites == null)
            {
                return null;
            }

            var testSuites = healthMonitorCS.TestSuites
                .Select(x => Type.GetType(x.Type))
                .Where(x => x != null)
                .ToArray();

            return testSuites;
        }

        public TestReport Run()
        {
            var testSuiteTypes = GetTestSuiteTypes();
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

            foreach (var testSuiteType in testSuiteTypes)
            {
                var test = TestFixtureBuilder.BuildFrom(testSuiteType);
                testSuite.Tests.Add(test);
            }

            var testResult = testSuite.Run(new NullListener(), TestFilter.Empty);
            var testReport = GenerateTestReport(testResult);

            return testReport;
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
                    Success = testResult.IsSuccess
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
                foreach (TestResult childTestResult in testResult.Results)
                {
                    result.AddRange(VisitTestResult(childTestResult));
                }
            }

            return result;
        }
    }
}
