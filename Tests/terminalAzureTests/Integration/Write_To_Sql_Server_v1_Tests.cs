using System.Linq;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using NUnit.Framework;

namespace terminalAzureTests.Integration
{
    [Explicit]
    public class Write_To_Sql_Server_v1_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalAzure"; }
        }
    }
}
