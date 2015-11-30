using System;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using TerminalBase.Infrastructure;
using terminalFr8Core.Actions;

namespace terminalFr8CoreTests.Unit
{
    [TestFixture]
    [Category("Select_Fr8_Object_v1")]
    class SelectFr8Object_v1Tests : BaseTest
    {
        IDisposable _coreServer;
        Select_Fr8_Object_v1 select_Fr8_Object_v1;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();

            _coreServer = Fixtures.FixtureData.CreateCoreServer_ActivitiesController();
            select_Fr8_Object_v1 = new Select_Fr8_Object_v1();
        }

        [TearDown]
        public void TearDown()
        {
            if (_coreServer != null)
            {
                _coreServer.Dispose();
                _coreServer = null;
            }
        }

        [Test]
        public async void Evaluate_IsValidJSONResponse_For_InitialRequest()
        {
            ActionDTO curActionDTO = FixtureData.TestActionDTOSelectFr8ObjectInitial();
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);
            AuthorizationTokenDO curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(curActionDTO.AuthToken);
            var action = await select_Fr8_Object_v1.Configure(curActionDO,curAuthTokenDO);
         
            Assert.NotNull(action);
            Assert.AreEqual(2, ObjectFactory.GetInstance<ICrateManager>().GetStorage(curActionDO.CrateStorage).Count);
        }

        [Test]
        public async void Evaluate_IsValidJSONResponse_For_FollowupRequest_RouteSelected()
        {
            ActionDTO curActionDTO = FixtureData.TestActionDTOSelectFr8ObjectFollowup("19");
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);
            AuthorizationTokenDO curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(curActionDTO.AuthToken);
            var action = await select_Fr8_Object_v1.Configure(curActionDO, curAuthTokenDO);

            Assert.NotNull(action);
           // Assert.AreEqual(2, actionDTO.CrateStorage.CrateDTO.Count);
        }

        [Test]
        public async void Evaluate_IsValidJSONResponse_For_FollowupRequest_ContainerSelected()
        {
            ActionDTO curActionDTO = FixtureData.TestActionDTOSelectFr8ObjectFollowup("21");
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);
            AuthorizationTokenDO curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(curActionDTO.AuthToken);
            var action = await select_Fr8_Object_v1.Configure(curActionDO, curAuthTokenDO);


            Assert.NotNull(action);
            //Assert.AreEqual(2, actionDTO.CrateStorage.CrateDTO.Count);
        }
    }

	/// <summary>
	/// Mark test case class with [Explicit] attiribute.
	/// It prevents test case from running when CI is building the solution,
	/// but allows to trigger that class from HealthMonitor.
	/// </summary>
	[Explicit]
	public class Select_Fr8_Object_v1Tests : BaseHealthMonitorTest
	{
		public override string TerminalName
		{
			get { return "terminalFr8Core"; }
		}

		[Test]
		public void Check_Initial_Configuration_Crate_Structure()
		{
			var configureUrl = GetTerminalConfigureUrl();

			var activityTemplate = new ActivityTemplateDTO
			{
				Id = 1,
				Name = "Select_Fr8_Object_TEST",
				Version = "1"
			};

			var requestActionDTO = new ActionDTO
			{
				Id = Guid.NewGuid(),
				Name = "Select_Fr8_Object",
				Label = "Select Fr8 Object",
				ActivityTemplate = activityTemplate,
				ActivityTemplateId = activityTemplate.Id
			};

			var responseActionDTO = HttpPostAsync<ActionDTO, ActionDTO>(configureUrl, requestActionDTO).Result;

			Assert.NotNull(responseActionDTO);
			Assert.NotNull(responseActionDTO.CrateStorage);
			Assert.NotNull(responseActionDTO.CrateStorage.Crates);

			var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);

			Assert.AreEqual(2, crateStorage.Count);

			Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
			Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "Select Fr8 Object"));
		}
	}
}
