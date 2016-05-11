using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Newtonsoft.Json;
using NUnit.Framework;
using HealthMonitor.Utility;
using Hub.Managers;
using terminalTests.Fixtures;

namespace terminalFr8CoreTests.Integration
{
    [Explicit]
    public class MapFields_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        private void AssertCrateStructure(ICrateStorage storage)
        {
            Assert.AreEqual(1, storage.CratesOfType<FieldDescriptionsCM>(x => x.Label == "Downstream Terminal-Provided Fields").Count());
            Assert.AreEqual(1, storage.CratesOfType<FieldDescriptionsCM>(x => x.Label == "Upstream Terminal-Provided Fields").Count());
            Assert.AreEqual(1, storage.CratesOfType<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls").Count());
        }

        private void AssertCrateContent_Initial(ICrateStorage storage)
        {
            var downstream = storage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "Downstream Terminal-Provided Fields").Single();
            Assert.AreEqual(0, downstream.Fields.Count());

            var upstream = storage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "Upstream Terminal-Provided Fields").Single();
            Assert.AreEqual(0, upstream.Fields.Count());

            var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            Assert.AreEqual(1, controls.Controls.Count);
            Assert.IsTrue(controls.Controls[0] is TextBlock);
        }

        private void AssertCrateContent_FollowUp(ICrateStorage storage)
        {
            var downstream = storage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "Downstream Terminal-Provided Fields").Single();
            Assert.AreEqual(1, downstream.Fields.Count());
            Assert.AreEqual("C", downstream.Fields[0].Key);
            Assert.AreEqual("D", downstream.Fields[0].Value);

            var upstream = storage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "Upstream Terminal-Provided Fields").Single();
            Assert.AreEqual(1, upstream.Fields.Count());
            Assert.AreEqual("A", upstream.Fields[0].Key);
            Assert.AreEqual("B", upstream.Fields[0].Value);

            var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            Assert.AreEqual(2, controls.Controls.Count);
            Assert.IsTrue(controls.Controls[0] is TextBlock);  
            Assert.IsTrue(controls.Controls[1] is MappingPane);            
        }

        private async Task<ActivityDTO> ConfigureWithUpstreamDownstreamData()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = HealthMonitor_FixtureData.MapFields_v1_InitialConfiguration_Fr8DataDTO();

            AddUpstreamCrate(
                dataDTO,
                new FieldDescriptionsCM(
                    new FieldDTO("A", "B")
                )
            );

            AddDownstreamCrate(
                dataDTO,
                new FieldDescriptionsCM(
                    new FieldDTO("C", "D")
                )
            );

            var responseActivityDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);

            return responseActivityDTO;
        }

        private async Task<ActivityDTO> ConfigureWithUpstreamDownstreamControlData()
        {
            var activityDTO = await ConfigureWithUpstreamDownstreamData();

            using (var crateStorage = Crate.GetUpdatableStorage(activityDTO))
            {
                var storage = crateStorage;
                var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();

                var mappingPane = (MappingPane)controls.Controls[1];
                var mapping = new List<FieldDTO>()
                {
                    new FieldDTO("A", "C")
                };

                mappingPane.Value = JsonConvert.SerializeObject(mapping);
            }

            var configureUrl = GetTerminalConfigureUrl();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            activityDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);

            return activityDTO;
        }

        [Test]
        public async Task MapFields_Configure_No_Upstream_Downstream_Data()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = HealthMonitor_FixtureData
                .MapFields_v1_InitialConfiguration_Fr8DataDTO();

            dataDTO.ActivityDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
            var crateStorage = Crate.GetStorage(dataDTO.ActivityDTO);

            AssertCrateStructure(crateStorage);
            AssertCrateContent_Initial(crateStorage);
        }

        [Test]
        public async Task MapFields_Configure_With_Upstream_Downstream_Data()
        {
            var activityDTO = await ConfigureWithUpstreamDownstreamData();
            var crateStorage = Crate.GetStorage(activityDTO);

            AssertCrateStructure(crateStorage);
            AssertCrateContent_FollowUp(crateStorage);
        }

        [Test]
        public async Task MapFields_Configure_With_Upstream_Downstream_Control_Data()
        {
            var activityDTO = await ConfigureWithUpstreamDownstreamControlData();

            var configureUrl = GetTerminalConfigureUrl();
            var dataDTO = new Fr8DataDTO { ActivityDTO = activityDTO };
            activityDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);

            var crateStorage = Crate.GetStorage(activityDTO);
            AssertCrateStructure(crateStorage);
            AssertCrateContent_FollowUp(crateStorage);

            var followUpControls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            var followUpMappingPane = (MappingPane)followUpControls.Controls[1];
            var followUpMapping = JsonConvert.DeserializeObject<List<FieldDTO>>(
                followUpMappingPane.Value
            );

            Assert.AreEqual(1, followUpMapping.Count);
            Assert.AreEqual("A", followUpMapping[0].Key);
            Assert.AreEqual("C", followUpMapping[0].Value);
        }

        [Test]
        public void MapFields_Run()
        {
            //var actionDTO = await ConfigureWithUpstreamDownstreamControlData();

            //var runUrl = GetTerminalRunUrl();

            //AddOperationalStateCrate(actionDTO, new OperationalStateCM());

            //var payload = await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);

            //Assert.NotNull(payload);

            //var crateStorage = Crate.GetStorage(payload);
            //Assert.AreEqual(2, crateStorage.Count);
            //Assert.AreEqual(1, crateStorage.CratesOfType<StandardPayloadDataCM>().Count());

            //var fields = crateStorage.CrateContentsOfType<StandardPayloadDataCM>().Single().AllValues().ToList();
            //Assert.AreEqual(1, fields.Count);
            //Assert.AreEqual("A", fields[0].Key);
            //Assert.AreEqual("C", fields[0].Value);
        }
    }
}
