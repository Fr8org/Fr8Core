using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthMonitor.Utility;
using NUnit.Framework;

namespace terminalTests.Integration
{
    /// <summary>
	/// Mark test case class with [Explicit] attiribute.
	/// It prevents test case from running when CI is building the solution,
	/// but allows to trigger that class from HealthMonitor.
	/// </summary>
	[Explicit]
    class TestAndBranch_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        [Test]
        public void Check_Initial_Configuration_Crate_Structure()
        {

        }



    }
}
