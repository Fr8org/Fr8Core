using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Crates;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Enums;
using Hub.Interfaces;
using Hub.Managers;
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace pluginBaseTests.BaseClasses
{

    [TestFixture]
    [Category("BasePluginAction")]
    public class BasePluginActionTests : BaseTest
    {
        IDisposable _coreServer;
        BasePluginAction _basePluginAction;
        private ICrateManager _crateManager;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _basePluginAction = new BasePluginAction();
            _coreServer = pluginBaseTests.Fixtures.FixtureData.CreateCoreServer_ActivitiesController();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
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
        public async void ProcessConfigurationRequest_CrateStroageIsNull_ShouldCrateNullStorage()
        {
            //Arrange
            ActionDTO curActionDTO = FixtureData.TestActionDTO1();
            ConfigurationEvaluator curConfigurationEvaluator = EvaluateReceivedRequest;
            object[] parameters = new object[] { curActionDTO, curConfigurationEvaluator };

            //Act
            var result = await (Task<ActionDTO>) ClassMethod.Invoke(typeof(BasePluginAction), "ProcessConfigurationRequest", parameters);

            
            //Assert
            Assert.AreEqual(_crateManager.FromDto(curActionDTO.CrateStorage), _crateManager.FromDto(result.CrateStorage));
        }


        [Test]
        public async void ProcessConfigurationRequest_ConfigurationRequestTypeIsFollowUp_ReturnsExistingCrateStorage()
        {
            //Arrange
            ActionDO curAction = FixtureData.TestConfigurationSettingsDTO1();
            ActionDTO curActionDTO = Mapper.Map<ActionDTO>(curAction);
            ConfigurationEvaluator curConfigurationEvaluator = EvaluateReceivedRequest;

            object[] parameters = new object[] { curActionDTO, curConfigurationEvaluator };

            //Act
            var result = await (Task<ActionDTO>)ClassMethod.Invoke(typeof(BasePluginAction), "ProcessConfigurationRequest", parameters);

            //Assert
            Assert.AreEqual(_crateManager.FromDto(curActionDTO.CrateStorage).Count, _crateManager.FromDto(result.CrateStorage).Count);
            Assert.AreEqual(_crateManager.FromDto(curActionDTO.CrateStorage).First().ManifestType, _crateManager.FromDto(result.CrateStorage).First().ManifestType);

        }

        [Test]
        public void PackControlsCrate_ReturnsStandardConfigurationControls()
        {
            //Arrange
            object[] parameters = new object[] { FixtureData.FieldDefinitionDTO1() };

            //Act
            var result = (Crate)ClassMethod.Invoke(typeof(BasePluginAction), "PackControlsCrate", parameters);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Get<StandardConfigurationControlsCM>() != null);
        }


        [Test]
        public void MergeContentFields_ReturnsStandardDesignTimeFieldsMS()
        {
            var result = _basePluginAction.MergeContentFields(FixtureData.TestCrateDTO1());
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Fields.Count);
        }



        //TestActionTree
        [Test]
        public async void GetDesignTimeFields_CrateDirectionIsUpstream_ReturnsMergeDesignTimeFields()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.RouteNodeRepository.Add(FixtureData.TestActionTree());
                uow.SaveChanges();

                ActionDO curAction = FixtureData.TestAction57();

                var result = await _basePluginAction.GetDesignTimeFields(
                    curAction.Id, GetCrateDirection.Upstream);
                Assert.NotNull(result);
                Assert.AreEqual(16, result.Fields.Count);
            }
        }

        [Test]
        public async void GetDesignTimeFields_CrateDirectionIsDownstream_ReturnsMergeDesignTimeFields()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.RouteNodeRepository.Add(FixtureData.TestActionTree());
                uow.SaveChanges();

                ActionDO curAction = FixtureData.TestAction57();

                var result = await _basePluginAction.GetDesignTimeFields(
                    curAction.Id, GetCrateDirection.Downstream);
                Assert.NotNull(result);
                Assert.AreEqual(18, result.Fields.Count);
            }
        }


        private ConfigurationRequestType EvaluateReceivedRequest(ActionDTO curActionDTO)
        {
            if (_crateManager.IsEmptyStorage(curActionDTO.CrateStorage))
                return ConfigurationRequestType.Initial;
            return ConfigurationRequestType.Followup;
        }

       

    }
}
