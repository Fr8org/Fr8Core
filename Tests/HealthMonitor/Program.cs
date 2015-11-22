using System;
using NUnit.Core;

namespace HealthMonitor
{
    public class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            CoreExtensions.Host.InitializeService();

            var testPackageFactory = new NUnitTestPackageFactory();
            var package = testPackageFactory.CreateTestPackage();

            var testRunner = new NUnitTestRunner();
            var report = testRunner.Run(package);

            var reportNotifier = new TestReportNotifier();
            reportNotifier.Notify(report);
        }
    }
}
