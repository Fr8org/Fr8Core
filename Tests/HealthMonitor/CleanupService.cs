using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitor
{
    public class CleanupService
    {
        public void LaunchCleanup(string connectionString)
        {
            TimeSpan timeout = new TimeSpan(0, 10, 0); // max run time

            string cleanUpScript = string.Empty;
            string cleanUpScriptPath = string.Empty;
            string commandText = string.Empty;
            string scriptName = "CleanUpAfterTests.ps1";

            string rootPath = UpNLevels(Environment.CurrentDirectory, 4);
            string sqlScript = Path.Combine(rootPath, "_PowerShellScripts", scriptName);
            if (File.Exists(sqlScript))
                commandText = File.ReadAllText(sqlScript);
            else
            {
                //Check alternative location (for HM deployed as a Web Job)
                rootPath = Environment.CurrentDirectory;
                sqlScript = Path.Combine(rootPath, scriptName);
                if (File.Exists(sqlScript))
                {
                    commandText = File.ReadAllText(sqlScript);
                }
                else
                {
                    throw new FileNotFoundException($"The PowerShell script is not found in this location: {sqlScript}");
                }
            }

            Console.WriteLine("Running clean-up script...");
            using (PowerShell ps = PowerShell.Create())
            {
                // the streams (Error, Debug, Progress, etc) are available on the PowerShell instance.
                // we can review them during or after execution.
                // we can also be notified when a new item is written to the stream (like this):
                ps.Streams.Error.DataAdded += error_DataAdded;
                ps.Streams.Debug.DataAdded += debug_DataAdded;

                ps.AddCommand("Set-ExecutionPolicy").AddParameter("ExecutionPolicy", "RemoteSigned").AddParameter("Scope", "Process").Invoke();

                Pipeline pipeLine = ps.Runspace.CreatePipeline();
                Command cleanCommand = new Command(cleanUpScript, true);
                cleanCommand.Parameters.Add("connectionString", connectionString);
                cleanCommand.Parameters.Add("dbName", "");
                pipeLine.Commands.Add(cleanCommand);
                pipeLine.Commands.Add("Out-String");

                DateTime startTime = DateTime.UtcNow;
                pipeLine.InvokeAsync();
                while (pipeLine.PipelineStateInfo.State == PipelineState.Running || pipeLine.PipelineStateInfo.State == PipelineState.Stopping)
                {
                    if (startTime + timeout < DateTime.UtcNow)
                    {
                        //Timeout condition
                        Console.WriteLine("Operation timeout, exiting");
                        break;
                    }
                    Thread.Sleep(500);
                }

                var output = pipeLine.Output.ReadToEnd();

                foreach (PSObject outputItem in output)
                {
                    Console.WriteLine(outputItem.BaseObject?.ToString());
                }

                if (pipeLine.PipelineStateInfo.State == PipelineState.Failed)
                {
                    Console.WriteLine("Execution completed with error. Reason: " + pipeLine.PipelineStateInfo.Reason.Message);
                }
                else
                {
                    Console.WriteLine("Execution has finished. The pipeline state: " + pipeLine.PipelineStateInfo.State);
                }
            }
        }


        private void error_DataAdded(object sender, DataAddedEventArgs e)
        {
            Console.WriteLine(((PSDataCollection<ErrorRecord>)sender)[e.Index].ToString());
        }

        private void debug_DataAdded(object sender, DataAddedEventArgs e)
        {
            Console.WriteLine(((PSDataCollection<DebugRecord>)sender)[e.Index].ToString());
        }

        private static string UpNLevels(string path, int levels)
        {
            int index = path.LastIndexOf('\\', path.Length - 1, path.Length);
            if (index <= 3)
                return string.Empty;
            string result = path.Substring(0, index);
            if (levels > 1)
            {
                result = UpNLevels(result, levels - 1);
            }
            return result;
        }
    }
}
