using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;

namespace terminalFr8CoreTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class Select_Fr8_Object_v1Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        [Test]
        public void Check_Initial_Configuration_Crate_Structure()
        {
            //Assert.AreEqual(1, 2);
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = CreateRequestActivityFixture();

            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };
            var responseActionDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);

            Assert.AreEqual(2, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "Select Fr8 Object"));

            var configCrate = crateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls")
                .SingleOrDefault();

            ValidateConfigurationCrateStructure(configCrate);

            var designTimeCrate = crateStorage
                .CrateContentsOfType<KeyValueListCM>(x => x.Label == "Select Fr8 Object")
                .SingleOrDefault();
            ValidateFr8ObjectCrateStructure(designTimeCrate);
        }

        [Test]
        public void Check_FollowUp_Configuration_Crate_Structure_When_Plans_Option_Is_Selected()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = CreateRequestActivityFixture();

            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };
            var responseActionDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;

            SetPlansOptionSelected(responseActionDTO);

            dataDTO.ActivityDTO = responseActionDTO;
            responseActionDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);

            Assert.AreEqual(3, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "Select Fr8 Object"));
            Assert.AreEqual(1, crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Label == "StandardFr8PlansCM"));

            var configCrate = crateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls")
                .SingleOrDefault();

            ValidateConfigurationCrateStructure(configCrate);

            var configurationControl = (DropDownList)configCrate.Controls.FirstOrDefault();

            Assert.AreEqual("19", configurationControl.Value);
            Assert.AreEqual("Plans", configurationControl.selectedKey);

            var selectFr8ObjectDesignTimeCrate = crateStorage
                .CrateContentsOfType<KeyValueListCM>(x => x.Label == "Select Fr8 Object")
                .SingleOrDefault();

            ValidateFr8ObjectCrateStructure(selectFr8ObjectDesignTimeCrate);

            var fr8PlansDesignTimeCrate = crateStorage
                .CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "StandardFr8PlansCM")
                .SingleOrDefault();

            Assert.AreEqual(8, fr8PlansDesignTimeCrate.Fields.Count);
            Assert.AreEqual("Select Fr8 Object Properties", configurationControl.Label);

            var fr8PlansCrateFields = fr8PlansDesignTimeCrate.Fields;

            Assert.AreEqual("CreateDate", fr8PlansCrateFields[0].Name);
            Assert.AreEqual("DateTime", fr8PlansCrateFields[0].Label);

            Assert.AreEqual("LastUpdated", fr8PlansCrateFields[1].Name);
            Assert.AreEqual("DateTime", fr8PlansCrateFields[1].Label);

            Assert.AreEqual("Description", fr8PlansCrateFields[2].Name);
            Assert.AreEqual("String", fr8PlansCrateFields[2].Label);

            Assert.AreEqual("Name", fr8PlansCrateFields[3].Name);
            Assert.AreEqual("String", fr8PlansCrateFields[3].Label);

            Assert.AreEqual("Ordering", fr8PlansCrateFields[4].Name);
            Assert.AreEqual("Int32", fr8PlansCrateFields[4].Label);

            Assert.AreEqual("PlanState", fr8PlansCrateFields[5].Name);
            Assert.AreEqual("String", fr8PlansCrateFields[5].Label);

            Assert.AreEqual("SubPlans", fr8PlansCrateFields[6].Name);
            Assert.AreEqual("System.Collections.Generic.List`1[[Fr8.Infrastructure.Data.DataTransferObjects.SubplanDTO, Fr8Infrastructure.NET, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]", fr8PlansCrateFields[6].Label);

            Assert.AreEqual("ManifestType", fr8PlansCrateFields[7].Name);
            Assert.AreEqual("CrateManifestType", fr8PlansCrateFields[7].Label);
        }

        [Test]
        public void Check_FollowUp_Configuration_Crate_Structure_When_Containers_Option_Is_Selected()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = CreateRequestActivityFixture();
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };
            var responseActionDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;

            SetContainersOptionSelected(responseActionDTO);
            dataDTO.ActivityDTO = responseActionDTO;
            responseActionDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);

            Assert.AreEqual(3, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "Select Fr8 Object"));
            Assert.AreEqual(1, crateStorage.CratesOfType<FieldDescriptionsCM>().Count(x => x.Label == "StandardFr8ContainersCM"));

            var configCrate = crateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls")
                .SingleOrDefault();

            ValidateConfigurationCrateStructure(configCrate);

            var configurationControl = (DropDownList)configCrate.Controls.FirstOrDefault();

            Assert.AreEqual("21", configurationControl.Value);
            Assert.AreEqual("Containers", configurationControl.selectedKey);

            var selectFr8ObjectDesignTimeCrate = crateStorage
                .CrateContentsOfType<KeyValueListCM>(x => x.Label == "Select Fr8 Object")
                .SingleOrDefault();

            ValidateFr8ObjectCrateStructure(selectFr8ObjectDesignTimeCrate);

            var fr8ContainersDesignTimeCrate = crateStorage
                .CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "StandardFr8ContainersCM")
                .SingleOrDefault();

            Assert.AreEqual("Select Fr8 Object Properties", configurationControl.Label);
            Assert.AreEqual(5, fr8ContainersDesignTimeCrate.Fields.Count);

            var fr8ContainersCrateFields = fr8ContainersDesignTimeCrate.Fields;

            Assert.AreEqual("Name", fr8ContainersCrateFields[0].Name);
            Assert.AreEqual("String", fr8ContainersCrateFields[0].Label);

            Assert.AreEqual("Description", fr8ContainersCrateFields[1].Name);
            Assert.AreEqual("String", fr8ContainersCrateFields[1].Label);

            Assert.AreEqual("CreatedDate", fr8ContainersCrateFields[2].Name);
            Assert.AreEqual("DateTime", fr8ContainersCrateFields[2].Label);

            Assert.AreEqual("LastUpdated", fr8ContainersCrateFields[3].Name);
            Assert.AreEqual("DateTime", fr8ContainersCrateFields[3].Label);

            Assert.AreEqual("ManifestType", fr8ContainersCrateFields[4].Name);
            Assert.AreEqual("CrateManifestType", fr8ContainersCrateFields[4].Label);
        }

        [Test]
        public void Run_With_Plan_Payload()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = CreateRequestActivityFixture();

            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };

            var responseActionDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;

            SetPlansOptionSelected(responseActionDTO);

            var runUrl = GetTerminalRunUrl();

            dataDTO.ActivityDTO = responseActionDTO;

            AddPayloadCrate(dataDTO, new StandardFr8PlansCM
            {
                CreateDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Description = "Some description",
                Name = "Some name",
                Ordering = 1,
                PlanState = "Some state",
                SubPlans = new List<SubplanDTO>()
            });
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());


            var runResponse = HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO).Result;

            Assert.NotNull(runResponse);
        }

        [Test]
        public void Run_With_Container_Payload()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = CreateRequestActivityFixture();

            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };

            var responseActionDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;

            SetContainersOptionSelected(responseActionDTO);

            var runUrl = GetTerminalRunUrl();

            dataDTO.ActivityDTO = responseActionDTO;

            AddPayloadCrate(dataDTO, new StandardFr8ContainersCM()
            {
                Name = "Some name",
                Description = "Some description",
                LastUpdated = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            });
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            var runResponse = HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO).Result;

            Assert.NotNull(runResponse);
        }

        private ActivityTemplateSummaryDTO CreateActivityTemplateFixture()
        {
            var activityTemplate = new ActivityTemplateSummaryDTO
            {
                Name = "Select_Fr8_Object_TEST",
                Version = "1"
            };

            return activityTemplate;
        }

        private ActivityDTO CreateRequestActivityFixture()
        {
            var activityTemplate = CreateActivityTemplateFixture();

            var requestActionDTO = new ActivityDTO
            {
                Id = Guid.NewGuid(),
                Label = "Select Fr8 Object",
                ActivityTemplate = activityTemplate,
                AuthToken = null
            };

            return requestActionDTO;
        }

        private void SetPlansOptionSelected(ActivityDTO responseActionDTO)
        {
            using (var crateStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();

                var dropdownList = (DropDownList)controls.Controls[0];
                dropdownList.selectedKey = "Plans";
                dropdownList.Value = "19";
            }
        }

        private void SetContainersOptionSelected(ActivityDTO responseActionDTO)
        {
            using (var crateStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();

                var dropdownList = (DropDownList)controls.Controls[0];
                dropdownList.selectedKey = "Containers";
                dropdownList.Value = "21";
            }
        }

        private void ValidateConfigurationCrateStructure(StandardConfigurationControlsCM configCrate)
        {
            var controls = configCrate.Controls;

            Assert.AreEqual(1, controls.Count);

            var configurationControl = controls.FirstOrDefault();

            Assert.IsInstanceOf<DropDownList>(configurationControl);
            Assert.AreEqual("Selected_Fr8_Object", configurationControl.Name);


            var configurationControlEvents = configurationControl.Events;

            Assert.AreEqual(1, configurationControlEvents.Count);

            var configurationControlEvent = configurationControlEvents.FirstOrDefault();

            Assert.AreEqual("onChange", configurationControlEvent.Name);
            Assert.AreEqual("requestConfig", configurationControlEvent.Handler);

            Assert.NotNull(configurationControl.Source);

            var configurationControlSourceField = configurationControl.Source;

            Assert.AreEqual("Select Fr8 Object", configurationControlSourceField.Label);
            Assert.AreEqual("Field Description", configurationControlSourceField.ManifestType);
        }

        private static void ValidateFr8ObjectCrateStructure(KeyValueListCM designTimeCrate)
        {
            var designTimeFields = designTimeCrate.Values;

            Assert.AreEqual(2, designTimeFields.Count);

            var field1 = designTimeFields[0];

            Assert.AreEqual("Plans", field1.Key);
            Assert.AreEqual("19", field1.Value);

            var field2 = designTimeFields[1];

            Assert.AreEqual("Containers", field2.Key);
            Assert.AreEqual("21", field2.Value);
        }
    }
}