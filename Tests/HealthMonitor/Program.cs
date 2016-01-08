using System;
using System.Linq;
using NUnit.Core;
using HealthMonitor.Configuration;
using System.Configuration;

namespace HealthMonitor
{
    public class Program
    {
        static void Main(string[] args)
        {
            var sendEmailReport = false;
            var appName = "Unspecified App";
            var selfHosting = true;

            if (args != null)
            {
                for (var i = 0; i < args.Length; ++i)
                {
                    if (args[i] == "--email-report")
                    {
                        sendEmailReport = true;
                    }
                    else if (i > 0 && args[i - 1] == "--app-name" && args[i] != null)
                    {
                        appName = args[i];
                    }
                    if (args[i] == "--self-hosting")
                    {
                        selfHosting = true;
                    }
                }
            }

            var selfHostInitializer = new SelfHostInitializer();
            if (selfHosting)
            {
                selfHostInitializer.Initialize();
            }

            try
            {
                new Program().Run(sendEmailReport, appName);
            }
            finally
            {
                if (selfHosting)
                {
                    selfHostInitializer.Dispose();
                }
            }
        }

        private void Run(bool sendEmailReport, string appName)
        {
            CoreExtensions.Host.InitializeService();

            // var testPackageFactory = new NUnitTestPackageFactory();
            // var package = testPackageFactory.CreateTestPackage();

            var testRunner = new NUnitTestRunner();
            // var report = testRunner.Run(package);
            var report = testRunner.Run();

            // System.IO.File.WriteAllText("c:\\temp\\fr8-report.html", htmlReport);

            if (sendEmailReport)
            {
                if (report.Tests.Any(x => !x.Success))
                {
                    var reportBuilder = new HtmlReportBuilder();
                    var htmlReport = reportBuilder.BuildReport(appName, report);

                    var reportNotifier = new TestReportNotifier();
                    reportNotifier.Notify(appName, htmlReport);
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Application: {0}", appName);
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
