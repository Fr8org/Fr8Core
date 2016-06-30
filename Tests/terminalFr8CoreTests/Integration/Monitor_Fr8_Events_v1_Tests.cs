using Fr8.Testing.Integration;
using NUnit.Framework;
using System;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using terminalFr8CoreTests.Fixtures;

namespace terminalTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("Integration.terminalFr8CoreTests")]
    class Monitor_Fr8_Events_v1_Tests : BaseTerminalIntegrationTest
    {

        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        [Test]
        public void Check_Initial_Configuration_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActivityDTO = FixtureData.MonitorFr8Event_InitialConfiguration_ActivityDTO();
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActivityDTO };
            var responseActivityDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;

            Assert.NotNull(responseActivityDTO);
            Assert.NotNull(responseActivityDTO.CrateStorage);
            Assert.NotNull(responseActivityDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActivityDTO.CrateStorage);
            Assert.AreEqual(2, crateStorage.Count);
        }

        [Test]
        public void Check_FollowUp_Configuration_Crate_Structure_Selected()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActivityDTO = FixtureData.MonitorFr8Event_InitialConfiguration_ActivityDTO();
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActivityDTO };
            var responseActivityDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;

            Assert.NotNull(responseActivityDTO);
            Assert.NotNull(responseActivityDTO.CrateStorage);
            Assert.NotNull(responseActivityDTO.CrateStorage.Crates);

        }

        [Test]
        public void Run_With_Plan_Payload()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestActivityDTO = FixtureData.MonitorFr8Event_InitialConfiguration_ActivityDTO();
            var dataDTO = new Fr8DataDTO { ActivityDTO = requestActivityDTO };
            var responseActivityDTO = HttpPostAsync<Fr8DataDTO, ActivityDTO>(configureUrl, dataDTO).Result;
            var runUrl = GetTerminalRunUrl();
            dataDTO.ActivityDTO = responseActivityDTO;
            AddPayloadCrate(
               dataDTO,
               new EventReportCM()
               {
                   EventPayload = new CrateStorage()
                   {
                        Fr8.Infrastructure.Data.Crates.Crate.FromContent(
                            "RouteActivatedReport",
                                RouteActivated()
                            )
                    },
                   EventNames = "RouteActivated"
               }
           );
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            var runResponse = HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO).Result;

            Assert.NotNull(runResponse);
        }

        private StandardLoggingCM RouteActivated()
        {
            StandardLoggingCM standardLoggingCM = new StandardLoggingCM();

            LogItemDTO logDTO = new LogItemDTO()
            {
                CustomerId = "1",
                Manufacturer = "Fr8 Company",
                Data = "",
                PrimaryCategory = "Plan",
                SecondaryCategory = "PlanState",
                Activity = "StateChanged",
                Status = "",
                CreateDate = new DateTime(),
                Type = "FactDO",
                Name = "RouteActivated",
                IsLogged = false,
                ObjectId = ""
            };

            standardLoggingCM.Item.Add(logDTO);

            return standardLoggingCM;
        }
    }
}
