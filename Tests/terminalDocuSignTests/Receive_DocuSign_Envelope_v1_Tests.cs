using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;

namespace terminalDocuSignTests
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Receive_DocuSign_Envelope_v1_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        [Test]
        public async void Example_UpstreamCrate()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Receive_DocuSign_Envelope_v1_Example_ActionDTO();
            AddUpstreamCrate(
                requestActionDTO,
                new StandardDesignTimeFieldsCM()
                {
                    Fields = new List<FieldDTO>()
                    {
                        new FieldDTO("TemplateId", "ea2258b2-2d80-4eca-9f40-6c5b5d5c5dda")
                    }
                }
            );

            var responseActionDTO = await JsonRestClient
                .PostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            var docuSignTemplateUserDefinedFieldsCrates = crateStorage
                .CratesOfType<StandardDesignTimeFieldsCM>(x => x.Label == "DocuSignTemplateUserDefinedFields");

            Assert.AreEqual(docuSignTemplateUserDefinedFieldsCrates.Count(), 1);
            // DocuSignTemplateUserDefinedFields
        }
    }
}
