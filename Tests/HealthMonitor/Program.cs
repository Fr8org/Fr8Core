using System;
using System.Linq;
using NUnit.Core;

namespace HealthMonitor
{
    public class Program
    {
        static void Main(string[] args)
        {
            var sendEmailReport = args != null && args.Contains("--email-report");

            new Program().Run(sendEmailReport);
        }

        private void Run(bool sendEmailReport)
        {
            CoreExtensions.Host.InitializeService();

            // var testPackageFactory = new NUnitTestPackageFactory();
            // var package = testPackageFactory.CreateTestPackage();

            var testRunner = new NUnitTestRunner();
            // var report = testRunner.Run(package);
            var report = testRunner.Run();

            var reportBuilder = new HtmlReportBuilder();
            var htmlReport = reportBuilder.BuildReport(report);

            // System.IO.File.WriteAllText("c:\\temp\\fr8-report.html", htmlReport);

            if (sendEmailReport)
            {
                var reportNotifier = new TestReportNotifier();
                reportNotifier.Notify(htmlReport);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Integration tests result: {0} / {1} passed", report.Tests.Count(x => x.Success), report.Tests.Count());
            Console.ForegroundColor = ConsoleColor.Gray;

            foreach (var test in report.Tests.Where(x => !x.Success))
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("----------------------------------------");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Integration Test Failure: {0}", test.Name);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Message: {0}", test.Message);
                Console.WriteLine("StackTrace: {0}", test.StackTrace);
            }

            var errorCount = report.Tests.Count(x => !x.Success);
            Environment.Exit(errorCount);
        }
    }
}
