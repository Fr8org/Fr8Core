using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;
using terminalQuickBooks.Controllers;
using terminalQuickBooks.Services;
using terminalQuickBooksTests.Fixtures;

namespace terminalQuickBooksTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    internal class Create_Journal_Entry_v1Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalQuickBooks"; }
        }
    }
}
