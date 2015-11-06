using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using terminalFr8Core.Actions;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces;
using Hub.Managers;
using StructureMap;
using terminalFr8Core.Interfaces;
using AutoMapper;
using Data.Entities;

namespace terminalFr8CoreTests.Actions
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
}
