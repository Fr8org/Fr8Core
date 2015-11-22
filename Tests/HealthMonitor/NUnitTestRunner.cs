using System;
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

                // PrintTestResult(testResult);
            }
            else
            {
                throw new ApplicationException("Could not initialize NUnit.Core.SimpleTestRunner.");
            }
        }
    }
}
