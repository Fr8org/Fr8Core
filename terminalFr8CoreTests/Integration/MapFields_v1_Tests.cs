using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using terminalTests.Fixtures;

namespace terminalFr8CoreTests.Integration
{
    [Explicit]
    public class MapFields_v1_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        private void AssertCrateStructure(CrateStorage storage)
        {
            Assert.AreEqual(1, storage.CratesOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Downstream Terminal-Provided Fields").Count());
            Assert.AreEqual(1, storage.CratesOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Upstream Terminal-Provided Fields").Count());
            Assert.AreEqual(1, storage.CratesOfType<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls").Count());
        }

        private void AssertCrateContent_Initial(CrateStorage storage)
        {
            var downstream = storage.CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Downstream Terminal-Provided Fields").Single();
            Assert.AreEqual(0, downstream.Fields.Count());

            var upstream = storage.CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Upstream Terminal-Provided Fields").Single();
            Assert.AreEqual(0, upstream.Fields.Count());

            var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            Assert.AreEqual(1, controls.Controls.Count);
            Assert.IsTrue(controls.Controls[0] is TextBlock);
        }

        private void AssertCrateContent_FollowUp(CrateStorage storage)
        {
            var downstream = storage.CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Downstream Terminal-Provided Fields").Single();
            Assert.AreEqual(1, downstream.Fields.Count());
            Assert.AreEqual("C", downstream.Fields[0].Key);
            Assert.AreEqual("D", downstream.Fields[0].Value);

            var upstream = storage.CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Upstream Terminal-Provided Fields").Single();
            Assert.AreEqual(1, upstream.Fields.Count());
            Assert.AreEqual("A", upstream.Fields[0].Key);
            Assert.AreEqual("B", upstream.Fields[0].Value);

            var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            Assert.AreEqual(1, controls.Controls.Count);
            Assert.IsTrue(controls.Controls[0] is MappingPane);            
        }

        private async Task<ActionDTO> ConfigureWithUpstreamDownstreamData()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var actionDTO = HealthMonitor_FixtureData
                .MapFields_v1_InitialConfiguration_ActionDTO();

            AddUpstreamCrate(
                actionDTO,
                new StandardDesignTimeFieldsCM(
                    new FieldDTO("A", "B")
                )
            );

            AddDownstreamCrate(
                actionDTO,
                new StandardDesignTimeFieldsCM(
                    new FieldDTO("C", "D")
                )
            );

            actionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, actionDTO);

            return actionDTO;
        }

        private async Task<ActionDTO> ConfigureWithUpstreamDownstreamControlData()
        {
            var actionDTO = await ConfigureWithUpstreamDownstreamData();

            using (var updater = Crate.UpdateStorage(actionDTO))
            {
                var storage = updater.CrateStorage;
                var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();

                var mappingPane = (MappingPane)controls.Controls[0];
                var mapping = new List<FieldDTO>()
                {
                    new FieldDTO("A", "C")
                };

                mappingPane.Value = JsonConvert.SerializeObject(mapping);
            }

            var configureUrl = GetTerminalConfigureUrl();
            actionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, actionDTO);

            return actionDTO;
        }

        [Test]
        public async void MapFields_Configure_No_Upstream_Downstream_Data()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var actionDTO = HealthMonitor_FixtureData
                .MapFields_v1_InitialConfiguration_ActionDTO();

            actionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, actionDTO);
            var crateStorage = Crate.GetStorage(actionDTO);

            AssertCrateStructure(crateStorage);
            AssertCrateContent_Initial(crateStorage);
        }

        [Test]
        public async void MapFields_Configure_With_Upstream_Downstream_Data()
        {
            var actionDTO = await ConfigureWithUpstreamDownstreamData();
            var crateStorage = Crate.GetStorage(actionDTO);

            AssertCrateStructure(crateStorage);
            AssertCrateContent_FollowUp(crateStorage);
        }

        [Test]
        public async void MapFields_Configure_With_Upstream_Downstream_Control_Data()
        {
            var actionDTO = await ConfigureWithUpstreamDownstreamControlData();

            var configureUrl = GetTerminalConfigureUrl();
            actionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, actionDTO);

            var crateStorage = Crate.GetStorage(actionDTO);
            AssertCrateStructure(crateStorage);
            AssertCrateContent_FollowUp(crateStorage);

            var followUpControls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            var followUpMappingPane = (MappingPane)followUpControls.Controls[0];
            var followUpMapping = JsonConvert.DeserializeObject<List<FieldDTO>>(
                followUpMappingPane.Value
            );

            Assert.AreEqual(1, followUpMapping.Count);
            Assert.AreEqual("A", followUpMapping[0].Key);
            Assert.AreEqual("C", followUpMapping[0].Value);
        }

        [Test]
        public async void MapFields_Run()
        {
            var actionDTO = await ConfigureWithUpstreamDownstreamControlData();

            var runUrl = GetTerminalRunUrl();

            AddOperationalStateCrate(actionDTO, new OperationalStateCM());

            var payload = await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);

            Assert.NotNull(payload);

            var crateStorage = Crate.GetStorage(payload);
            Assert.AreEqual(2, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardPayloadDataCM>().Count());

            var fields = crateStorage.CrateContentsOfType<StandardPayloadDataCM>().Single().AllValues().ToList();
            Assert.AreEqual(1, fields.Count);
            Assert.AreEqual("A", fields[0].Key);
            Assert.AreEqual("C", fields[0].Value);
        }
    }
}
