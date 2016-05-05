using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthMonitor
{
    public class CleanupService
    {
        public void LaunchCleanup()
        {
            TimeSpan timeout = new TimeSpan(0, 10, 0); // max run time

            string rootPath = Utilities.MiscUtils.UpNLevels(Environment.CurrentDirectory, 4);
            string powerShellScriptsPath = Path.Combine(rootPath, "_PowerShellScripts");
            string cleanUpScriptPath = Path.Combine(powerShellScriptsPath, "CleanUpAfterTests.ps1");

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.Arguments = $"& '{cleanUpScriptPath}'";
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.OutputDataReceived += OutputDataReceived;
            process.ErrorDataReceived += OutputDataReceived;

            process.EnableRaisingEvents = true;
            bool started = process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            DateTime startTime = DateTime.UtcNow;

            while (!process.HasExited && startTime + timeout > DateTime.UtcNow)
            {

                System.Threading.Thread.Sleep(500);
            }
        }

        private static void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;
            Console.WriteLine("Cleanup Script:\\> " + e.Data);
        }
    }
}
