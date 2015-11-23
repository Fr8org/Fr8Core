using System;
using System.Collections.Generic;
using NUnit.Core;

namespace HealthMonitor
{
    public class NUnitTestRunner
    {
        public TestReport Run(TestPackage testPackage)
        {
            var runner = new SimpleTestRunner();
            if (runner.Load(testPackage))
            {
                var testResult = runner.Run(
                    new NullListener(),
                    TestFilter.Empty,
                    false,
                    LoggingThreshold.Off
                );

                var testReport = GenerateTestReport(testResult);
                return testReport;
            }
            else
            {
                throw new ApplicationException("Could not initialize NUnit.Core.SimpleTestRunner.");
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
