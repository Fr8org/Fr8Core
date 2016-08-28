using System;
using System.Linq;
using Fr8.Infrastructure.Data.Constants;
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
    public class SetDelay_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        [Test]
        public void Check_Initial_Configuration_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = CreateRequestActivityFixture();
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };
            var responseActionDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);

            Assert.AreEqual(2, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));

            var configCrate = crateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls")
                .SingleOrDefault();

            ValidateConfigurationCrateStructure(configCrate);
        }

        /// <summary>
        /// Set Delay action does nothing on follow up configuration
        /// </summary>
		[Test]
        public void Check_FollowUp_Configuration_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = CreateRequestActivityFixture();
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };
            var responseActionDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;
            dataDTO.ActivityDTO = responseActionDTO;
            SetDuration(responseActionDTO);

            responseActionDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);

            Assert.AreEqual(2, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));

            var configCrate = crateStorage
                .CrateContentsOfType<StandardConfigurationControlsCM>(x => x.Label == "Configuration_Controls")
                .SingleOrDefault();

            ValidateConfigurationCrateStructure(configCrate);

            var configurationControl = (Duration)configCrate.Controls.FirstOrDefault();

            Assert.AreEqual(0, configurationControl.Days);
            Assert.AreEqual(0, configurationControl.Hours);
            Assert.AreEqual(2, configurationControl.Minutes);
        }

        [Test]
        public async void Run_With_Plan_Payload_Initial_Run()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = CreateRequestActivityFixture();

            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };

            var responseActionDTO = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO);
            var operationalState = new OperationalStateCM();
            operationalState.CallStack.PushFrame(new OperationalStateCM.StackFrame());
            AddOperationalStateCrate(dataDTO, operationalState);
            var runUrl = GetTerminalRunUrl();
            dataDTO.ActivityDTO = responseActionDTO;
            var runResponse = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            Assert.NotNull(runResponse);
            var crateStorage = Crate.GetStorage(runResponse);
            var operationalStateCrate = crateStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.AreEqual(ActivityResponse.RequestSuspend.ToString(), operationalStateCrate.CurrentActivityResponse.Type);
        }

        [Test]
        public void Run_With_Plan_Payload_Second_Run()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = CreateRequestActivityFixture();
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActionDTO };

            dataDTO.ActivityDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;
            SetDuration(dataDTO.ActivityDTO);
            var runUrl = GetTerminalRunUrl();
            var operationalState = new OperationalStateCM();
            operationalState.CallStack.PushFrame(new OperationalStateCM.StackFrame { CurrentActivityExecutionPhase = OperationalStateCM.ActivityExecutionPhase.ProcessingChildren });
            operationalState.CallStack.PushFrame(new OperationalStateCM.StackFrame { CurrentActivityExecutionPhase = OperationalStateCM.ActivityExecutionPhase.WasNotExecuted });
            operationalState.CallStack.StoreLocalData("Delay","suspended");
            AddOperationalStateCrate(dataDTO, operationalState);

            var runResponse = HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO).Result;
            Assert.NotNull(runResponse);
            var crateStorage = Crate.GetStorage(runResponse);
            var operationalStateCrate = crateStorage.CrateContentsOfType<OperationalStateCM>().Single();
            Assert.AreEqual(ActivityResponse.Success.ToString(), operationalStateCrate.CurrentActivityResponse.Type);
        }

        private ActivityTemplateSummaryDTO CreateActivityTemplateFixture()
        {
            var activityTemplate = new ActivityTemplateSummaryDTO
            {
                Name = "Set_Delay_TEST",
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
                Label = "Delay Action Processing",
                ActivityTemplate = activityTemplate,
                AuthToken = null
            };

            return requestActionDTO;
        }

        private void SetDuration(ActivityDTO responseActionDTO)
        {
            using (var crateStorage = Crate.GetUpdatableStorage(responseActionDTO))
            {
                var controls = crateStorage
                    .CrateContentsOfType<StandardConfigurationControlsCM>()
                    .Single();

                var duration = (Duration)controls.Controls[0];
                duration.Days = 0;
                duration.Hours = 0;
                duration.Minutes = 2;
            }
        }


        private void ValidateConfigurationCrateStructure(StandardConfigurationControlsCM configCrate)
        {
            var controls = configCrate.Controls;

            Assert.AreEqual(1, controls.Count);

            var configurationControl = controls.FirstOrDefault();

            Assert.IsInstanceOf<Duration>(configurationControl);
            Assert.AreEqual("Delay_Duration", configurationControl.Name);
            Assert.AreEqual("Please enter delay duration", configurationControl.Label);

            var configurationControlEvents = configurationControl.Events;

            Assert.IsEmpty(configurationControlEvents);
            Assert.IsNull(configurationControl.Source);
        }
    }
}