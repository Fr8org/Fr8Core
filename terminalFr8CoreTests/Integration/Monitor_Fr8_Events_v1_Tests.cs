using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers.APIManagers.Transmitters.Restful;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    class Monitor_Fr8_Events_v1_Tests : BaseHealthMonitorTest
    {

        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        [Test]
        public void Check_Initial_Configuration_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActivityDTO = FixtureData.MonitorFr8Event_InitialConfiguration_ActionDTO();

            var responseActivityDTO = HttpPostAsync<ActivityDTO, ActivityDTO>(configureUrl, requestActivityDTO).Result;

            Assert.NotNull(responseActivityDTO);
            Assert.NotNull(responseActivityDTO.CrateStorage);
            Assert.NotNull(responseActivityDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActivityDTO.CrateStorage);
            Assert.AreEqual(3, crateStorage.Count);
        }

        [Test]
        public void Check_FollowUp_Configuration_Crate_Structure_Selected()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActivityDTO = FixtureData.MonitorFr8Event_InitialConfiguration_ActionDTO();
            var responseActivityDTO = HttpPostAsync<ActivityDTO, ActivityDTO>(configureUrl, requestActivityDTO).Result;

            Assert.NotNull(responseActivityDTO);
            Assert.NotNull(responseActivityDTO.CrateStorage);
            Assert.NotNull(responseActivityDTO.CrateStorage.Crates);

        }

        [Test]
        public void Run_With_Route_Payload()
        {
            var configureUrl = GetTerminalConfigureUrl();
            var requestActivityDTO = FixtureData.MonitorFr8Event_InitialConfiguration_ActionDTO();
            var responseActivityDTO = HttpPostAsync<ActivityDTO, ActivityDTO>(configureUrl, requestActivityDTO).Result;
            var runUrl = GetTerminalRunUrl();

            AddPayloadCrate(
               responseActivityDTO,
               new EventReportCM()
               {
                   EventPayload = new CrateStorage()
                   {
                        Data.Crates.Crate.FromContent(
                            "RouteActivatedReport",
                                RouteActivated()
                            )
                    },
                   EventNames = "RouteActivated"
               }
           );

            var runResponse = HttpPostAsync<ActivityDTO, PayloadDTO>(runUrl, responseActivityDTO).Result;

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
                PrimaryCategory = "Route",
                SecondaryCategory = "RouteState",
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
