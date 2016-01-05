using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StructureMap;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Managers;

namespace TerminalBase.Infrastructure
{
    public class TestMonitoringHubCommunicator : DataHubCommunicatorBase
    {
        protected override string LabelPrefix
        {
            get { return "HealthMonitor"; }
        }
    }
}
