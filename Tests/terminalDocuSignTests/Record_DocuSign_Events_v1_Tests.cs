using Data.Control;
using Data.Crates;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminalDocuSignTests
{
    [Explicit]
    public class Record_DocuSign_Events_v1_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private void AssertCrateTypes(CrateStorage crateStorage)
        {
            Assert.AreEqual(3, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<EventSubscriptionCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "Available Run-Time Objects"));
        }

        private void AssertControls(StandardConfigurationControlsCM control)
        {
            Assert.AreEqual(1, control.Controls.Count);

            // Assert that first control is a RadioButtonGroup 
            // with Label == "Monitor All DocuSign events"
            // with Value == "This Action doesn't require any configuration."
            Assert.IsTrue(control.Controls[0] is TextBlock);
            Assert.AreEqual("Monitor All DocuSign events", control.Controls[0].Name);
            Assert.AreEqual("This Action doesn't require any configuration.", control.Controls[0].Value);
        }

        private void AssertList(EventSubscriptionCM control)
        {

        }
    }
}
