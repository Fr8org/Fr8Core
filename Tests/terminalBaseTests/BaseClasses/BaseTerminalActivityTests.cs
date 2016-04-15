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
    [Category("BaseTerminalActivity")]
    public class BaseTerminalActivityTests : BaseTest
    {
        IDisposable _coreServer;
        BaseTerminalActivity _baseTerminalActivity;
        private ICrateManager _crateManager;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();
            ObjectFactory.Configure(x => x.For<IRestfulServiceClient>().Use<RestfulServiceClient>().SelectConstructor(() => new RestfulServiceClient()));
            _baseTerminalActivity = new BaseTerminalActivity();
            _baseTerminalActivity.HubCommunicator.Configure("terminal");
            _coreServer = terminalBaseTests.Fixtures.FixtureData.CreateCoreServer_ActivitiesController();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();

            FixtureData.AddTestActivityTemplate();
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
        public async Task ProcessConfigurationRequest_CrateStroageIsNull_ShouldCrateNullStorage()
        {
            //Arrange
            ActivityDTO curActionDTO = FixtureData.TestActivityDTO1();
            ConfigurationEvaluator curConfigurationEvaluator = EvaluateReceivedRequest;
            var curActivityDO = Mapper.Map<ActivityDO>(curActionDTO);
            var curAuthTokenDO = curActionDTO.AuthToken;
            object[] parameters = new object[] { curActivityDO, curConfigurationEvaluator, curAuthTokenDO };

            //Act
            var result = await (Task<ActivityDO>) ClassMethod.Invoke(typeof(BaseTerminalActivity), "ProcessConfigurationRequest", parameters);

            
            //Assert
            Assert.AreEqual(_crateManager.FromDto(curActionDTO.CrateStorage), _crateManager.GetStorage(result));
        }


        [Test]
        public async Task ProcessConfigurationRequest_ConfigurationRequestTypeIsFollowUp_ReturnsExistingCrateStorage()
        {
            //Arrange
            ActivityDO curAction = FixtureData.TestConfigurationSettingsDTO1();
            ActivityDTO curActionDTO = Mapper.Map<ActivityDTO>(curAction);
            ConfigurationEvaluator curConfigurationEvaluator = EvaluateReceivedRequest;
            var curActivityDO = Mapper.Map<ActivityDO>(curActionDTO);
            var curAuthTokenDO = curActionDTO.AuthToken;
            object[] parameters = new object[] { curActivityDO, curConfigurationEvaluator, curAuthTokenDO };

            //Act
            var result = await (Task<ActivityDO>)ClassMethod.Invoke(typeof(BaseTerminalActivity), "ProcessConfigurationRequest", parameters);

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
            var result = (Crate)ClassMethod.Invoke(typeof(BaseTerminalActivity), "PackControlsCrate", parameters);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Get<StandardConfigurationControlsCM>() != null);
        }

        //TestActionTree
        [Test]
        public async Task GetDesignTimeFields_CrateDirectionIsUpstream_ReturnsMergeDesignTimeFields()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.PlanRepository.Add(new PlanDO
                {
                    Name="Test plan",
                    PlanState = PlanState.Active,
                    ChildNodes = { FixtureData.TestActivity2Tree()}
                });
                uow.SaveChanges();

                ActivityDO curAction = FixtureData.TestAction257();

                var result = await _baseTerminalActivity.GetDesignTimeFields(curAction, CrateDirection.Upstream);
                Assert.NotNull(result);
                Assert.AreEqual(48, result.Fields.Count);
            }
        }

        [Test]
        public async Task GetDesignTimeFields_CrateDirectionIsDownstream_ReturnsMergeDesignTimeFields()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.PlanRepository.Add(new PlanDO
                {
                    Name = "Test plan",
                    PlanState = PlanState.Active,
                    ChildNodes = { FixtureData.TestActivity2Tree() }
                });
                uow.SaveChanges();

                ActivityDO curAction = FixtureData.TestAction257();

                var result = await _baseTerminalActivity.GetDesignTimeFields(curAction, CrateDirection.Downstream);
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
        public async Task BuildUpstreamManifestList_ReturnsListOfUpstreamManifestTypes()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.PlanRepository.Add(new PlanDO
                {
                    Name = "Test plan",
                    PlanState = PlanState.Active,
                    ChildNodes = { FixtureData.TestActivity2Tree() }
                });
                uow.SaveChanges();

                ActivityDO curAction = FixtureData.TestAction257();
                var manifestList = await _baseTerminalActivity.BuildUpstreamManifestList(curAction);

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
        public async Task BuildUpstreamCrateLabelList_ReturnsListOfUpstreamCrateLabels()
        {
            ObjectFactory.Configure(x => x.Forward<IRestfulServiceClient, RestfulServiceClient>());
            ObjectFactory.Configure(x => x.For<IRestfulServiceClient>().Use<RestfulServiceClient>().SelectConstructor(() => new RestfulServiceClient()));
            var test = ObjectFactory.GetInstance<IRestfulServiceClient>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.PlanRepository.Add(new PlanDO
                {
                    Name = "Test plan",
                    PlanState = PlanState.Active,
                    ChildNodes = { FixtureData.TestActivity2Tree() }
                });
                uow.SaveChanges();

                ActivityDO curAction = FixtureData.TestAction257();
                var crateLabelList = await _baseTerminalActivity.BuildUpstreamCrateLabelList(curAction);

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

        private ConfigurationRequestType EvaluateReceivedRequest(ActivityDO curActivityDO)
        {
            if (_crateManager.IsStorageEmpty(curActivityDO))
                return ConfigurationRequestType.Initial;
            return ConfigurationRequestType.Followup;
        }

    }
}
