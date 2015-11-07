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
            ActionDTO actionDTO = FixtureData.TestActionDTOSelectFr8ObjectInitial();
         

            var action = await select_Fr8_Object_v1.Configure(actionDTO);

            Assert.NotNull(action);
            Assert.AreEqual(2, ObjectFactory.GetInstance<ICrateManager>().FromDto(action.CrateStorage).Count);
        }

        [Test]
        public async void Evaluate_IsValidJSONResponse_For_FollowupRequest_RouteSelected()
        {
            ActionDTO actionDTO = FixtureData.TestActionDTOSelectFr8ObjectFollowup("19");
           
            var action = await select_Fr8_Object_v1.Configure(actionDTO);

            Assert.NotNull(action);
           // Assert.AreEqual(2, actionDTO.CrateStorage.CrateDTO.Count);
        }

        [Test]
        public async void Evaluate_IsValidJSONResponse_For_FollowupRequest_ContainerSelected()
        {
            ActionDTO actionDTO = FixtureData.TestActionDTOSelectFr8ObjectFollowup("21");
          
            var action = await select_Fr8_Object_v1.Configure(actionDTO);


            Assert.NotNull(action);
            //Assert.AreEqual(2, actionDTO.CrateStorage.CrateDTO.Count);
        }
    }
}
