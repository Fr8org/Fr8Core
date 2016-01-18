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
using Data.States;

using Hub.Interfaces;
using Hub.Managers;
using TerminalBase.Infrastructure;
using TerminalBase.BaseClasses;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using System.Collections.Generic;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Moq;
using System.Net.Http;

namespace terminalBaseTests.BaseClasses
{

    [TestFixture]
    [Category("BaseTerminalAction")]
    public class BaseTerminalActionTests : BaseTest
    {
        IDisposable _coreServer;
        BaseTerminalAction _baseTerminalAction;
        private ICrateManager _crateManager;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();
            ObjectFactory.Configure(x => x.For<IRestfulServiceClient>().Use<RestfulServiceClient>());
            _baseTerminalAction = new BaseTerminalAction();
            _coreServer = terminalBaseTests.Fixtures.FixtureData.CreateCoreServer_ActivitiesController();
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
            var curActionDO = Mapper.Map<ActionDO>(curActionDTO);
            var curAuthTokenDO = curActionDTO.AuthToken;
            object[] parameters = new object[] { curActionDO, curConfigurationEvaluator, curAuthTokenDO };

            //Act
            var result = await (Task<ActionDO>) ClassMethod.Invoke(typeof(BaseTerminalAction), "ProcessConfigurationRequest", parameters);

            
            //Assert
            Assert.AreEqual(_crateManager.FromDto(curActionDTO.CrateStorage), _crateManager.GetStorage(result));
        }


        [Test]
        public async void ProcessConfigurationRequest_ConfigurationRequestTypeIsFollowUp_ReturnsExistingCrateStorage()
        {
            //Arrange
            ActionDO curAction = FixtureData.TestConfigurationSettingsDTO1();
            ActionDTO curActionDTO = Mapper.Map<ActionDTO>(curAction);
            ConfigurationEvaluator curConfigurationEvaluator = EvaluateReceivedRequest;
            var curActionDO = Mapper.Map<ActionDO>(curActionDTO);
            var curAuthTokenDO = curActionDTO.AuthToken;
            object[] parameters = new object[] { curActionDO, curConfigurationEvaluator, curAuthTokenDO };

            //Act
            var result = await (Task<ActionDO>)ClassMethod.Invoke(typeof(BaseTerminalAction), "ProcessConfigurationRequest", parameters);

            //Assert
            Assert.AreEqual(_crateManager.FromDto(curActionDTO.CrateStorage).Count, _crateManager.GetStorage(result.CrateStorage).Count);
            Assert.AreEqual(_crateManager.FromDto(curActionDTO.CrateStorage).First().ManifestType, _crateManager.GetStorage(result.CrateStorage).First().ManifestType);

        }

        [Test]
        public void PackControlsCrate_ReturnsStandardConfigurationControls()
        {
            //Arrange
            object[] parameters = new object[] { FixtureData.FieldDefinitionDTO1() };

            //Act
            var result = (Crate)ClassMethod.Invoke(typeof(BaseTerminalAction), "PackControlsCrate", parameters);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Get<StandardConfigurationControlsCM>() != null);
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

                var result = await _baseTerminalAction.GetDesignTimeFields(
                    curAction.Id, CrateDirection.Upstream);
                Assert.NotNull(result);
                Assert.AreEqual(48, result.Fields.Count);
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

                var result = await _baseTerminalAction.GetDesignTimeFields(
                    curAction.Id, CrateDirection.Downstream);
                Assert.NotNull(result);
                Assert.AreEqual(54, result.Fields.Count);
            }
        }

        private static HashSet<CrateManifestType> ExcludedManifestTypes = new HashSet<CrateManifestType>()
        {
            ManifestDiscovery.Default.GetManifestType<StandardConfigurationControlsCM>(),
            ManifestDiscovery.Default.GetManifestType<EventSubscriptionCM>()
        };

        [Test]
        public async void BuildUpstreamManifestList_ReturnsListOfUpstreamManifestTypes()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.RouteNodeRepository.Add(FixtureData.TestActionTree());
                uow.SaveChanges();

                ActionDO curAction = FixtureData.TestAction57();
                var manifestList = await _baseTerminalAction.BuildUpstreamManifestList(curAction);

                Assert.NotNull(manifestList);
                Assert.AreEqual(manifestList.Count(), manifestList.Distinct().Count());
                Assert.AreEqual(3, manifestList.Count());

                foreach (var manifest in manifestList)
                {
                    Assert.IsFalse(ExcludedManifestTypes.Contains(manifest));
                }

            }
        }

        [Test]
        public async void BuildUpstreamCrateLabelList_ReturnsListOfUpstreamCrateLabels()
        {
            ObjectFactory.Configure(x => x.Forward<IRestfulServiceClient, RestfulServiceClient>());
            ObjectFactory.Configure(x => x.For<IRestfulServiceClient>().Use<RestfulServiceClient>());
            var test = ObjectFactory.GetInstance<IRestfulServiceClient>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.RouteNodeRepository.Add(FixtureData.TestActionTree());
                uow.SaveChanges();

                ActionDO curAction = FixtureData.TestAction57();
                var crateLabelList = await _baseTerminalAction.BuildUpstreamCrateLabelList(curAction);

                Assert.NotNull(crateLabelList);
                Assert.AreEqual(crateLabelList.Count(), crateLabelList.Distinct().Count());

                foreach (var crate in UtilitiesTesting.Fixtures.FixtureData.TestCrateDTO3())
                {
                    if (ExcludedManifestTypes.Contains(crate.ManifestType))
                    {
                        Assert.IsFalse(crateLabelList.Contains(crate.Label));

                    }
                    else
                    {
                        Assert.IsTrue(crateLabelList.Contains(crate.Label));
                    }
                }
            }
        }

        private ConfigurationRequestType EvaluateReceivedRequest(ActionDO curActionDO)
        {
            if (_crateManager.IsStorageEmpty(curActionDO))
                return ConfigurationRequestType.Initial;
            return ConfigurationRequestType.Followup;
        }

    }
}
