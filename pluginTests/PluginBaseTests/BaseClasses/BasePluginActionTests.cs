using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using NUnit.Framework;
using PluginBase.Infrastructure;
using StructureMap;
using System;
using System.Threading.Tasks;
using Core.Enums;
using PluginBase.BaseClasses;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace pluginTests.PluginBaseTests.Controllers
{

    [TestFixture]
    [Category("BasePluginAction")]
    public class BasePluginActionTests : BaseTest
    {
        IDisposable _coreServer;
        BasePluginAction _basePluginAction;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _basePluginAction = new BasePluginAction();
            _coreServer = FixtureData.CreateCoreServer_ActivitiesController();
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
        public async void ProcessConfigurationRequest_CrateStroageIsNull_ShouldNotCrateStorage()
        {
            //Arrange
            ActionDTO curActionDTO = FixtureData.TestActionDTO1();
            ConfigurationEvaluator curConfigurationEvaluator = EvaluateReceivedRequest;
            object[] parameters = new object[] { curActionDTO, curConfigurationEvaluator };

            //Act
            var result = await (Task<ActionDTO>) ClassMethod.Invoke(typeof(BasePluginAction), "ProcessConfigurationRequest", parameters);

            //Assert
            Assert.AreEqual(curActionDTO.CrateStorage.CrateDTO.Count, result.CrateStorage.CrateDTO.Count);
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
            Assert.AreEqual(curActionDTO.CrateStorage.CrateDTO.Count, result.CrateStorage.CrateDTO.Count);
            Assert.AreEqual(curActionDTO.CrateStorage.CrateDTO[0].ManifestType, result.CrateStorage.CrateDTO[0].ManifestType);

        }

        [Test]
        public void PackControlsCrate_ReturnsStandardConfigurationControls()
        {
            //Arrange
            object[] parameters = new object[] { FixtureData.FieldDefinitionDTO1() };

            ;
            //Act
            var result = (CrateDTO)ClassMethod.Invoke(typeof(BasePluginAction), "PackControlsCrate", parameters);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME, result.ManifestType);
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
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;
            if (curCrates.CrateDTO.Count == 0)
                return ConfigurationRequestType.Initial;
            return ConfigurationRequestType.Followup;
        }

       

    }
}
