using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoMapper;
using NUnit.Framework;
using Newtonsoft.Json;
using StructureMap;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Terminal;
using HubWeb.Controllers;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using terminalAzure;
using terminalDocuSign;
using terminalDocuSign.Infrastructure.AutoMapper;
using terminalDocuSign.Infrastructure.StructureMap;
using terminalDocuSign.Tests.Fixtures;
using TerminalBase.Infrastructure;

using DependencyType = Hub.StructureMap.StructureMapBootStrapper.DependencyType;
using terminalDocuSign.Infrastructure.StructureMap;
using terminalDocuSign.Infrastructure.AutoMapper;
using System.Security.Principal;
using Data.Control;
using Data.Crates;
using Hub.Managers;

namespace terminalIntegrationTests
{
    [TestFixture]
    [Category("TerminalIntegrationTests")]
    public partial class TerminalIntegrationTests : BaseTest
    {
        private IDisposable _coreServer;
        private IDisposable _docuSignServer;
        private IDisposable _dockyardCoreServer;
        private IDisposable _azureSqlServerServer;
        public ICrateManager _crateManager;

        private Fr8AccountDO _testUserAccount;
        private RouteDO _routeDO;
        private SubrouteDO _subrouteDO;
        //private ActionListDO _actionList;
        private AuthorizationTokenDO _authToken;
        private ActivityTemplateDO _waitForDocuSignEventActivityTemplate;
        private ActivityTemplateDO _testIncomingDataActivityTemplate;
        private ActivityTemplateDO _writeToSqlServerActivityTemplate;
        private ActivityTemplateDO _sendDocuSignEnvelopeActivityTemplate;

        /// <summary>
        /// Create _testUserAccount instance and store it in mock DB.
        /// Create _waitForDocuSignEventActivityTemplate instance and store it in mock DB.
        /// </summary>
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TerminalDocuSignMapBootstrapper.ConfigureDependencies(DependencyType.TEST);
            TerminalDataAutoMapperBootStrapper.ConfigureAutoMapper();
            TerminalBootstrapper.ConfigureTest();

            // these are integration tests, we are using a real transmitter
            ObjectFactory.Configure(c => c.For<ITerminalTransmitter>().Use<TerminalTransmitter>());

            _testUserAccount = FixtureData.TestUser1();

            _routeDO = FixtureData.Route_TerminalIntegration();
            _routeDO.Fr8Account = _testUserAccount;
            System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(_testUserAccount.Id), new string[] { "User" });

            _subrouteDO = FixtureData.Subroute_TerminalIntegration();
            _subrouteDO.ParentRouteNode = _routeDO;


            //_actionList = FixtureData.TestActionList_ImmediateActions();
            // _actionList.Subroute = _subrouteDO;

            _waitForDocuSignEventActivityTemplate =
                FixtureData.TestActivityTemplateDO_WaitForDocuSignEvent();

            _testIncomingDataActivityTemplate =
                FixtureData.TestActivityTemplateDO_TestIncomingData();

            _writeToSqlServerActivityTemplate =
                FixtureData.TestActivityTemplateDO_WriteToSqlServer();

            _sendDocuSignEnvelopeActivityTemplate =
                FixtureData.TestActivityTemplateDO_SendDocuSignEnvelope();
            _sendDocuSignEnvelopeActivityTemplate.Terminal = _waitForDocuSignEventActivityTemplate.Terminal;

            _authToken = FixtureData.AuthToken_TerminalIntegration();
            _authToken.Terminal = _waitForDocuSignEventActivityTemplate.Terminal;
            _authToken.UserDO = _testUserAccount;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //    uow.ActivityRepository.Add(_actionList);
                uow.ActivityTemplateRepository.Add(_waitForDocuSignEventActivityTemplate);
                uow.ActivityTemplateRepository.Add(_testIncomingDataActivityTemplate);
                uow.ActivityTemplateRepository.Add(_writeToSqlServerActivityTemplate);
                uow.ActivityTemplateRepository.Add(_sendDocuSignEnvelopeActivityTemplate);
                uow.UserRepository.Add(_testUserAccount);
                uow.AuthorizationTokenRepository.Add(_authToken);

                uow.RouteRepository.Add(_routeDO);
                uow.SubrouteRepository.Add(_subrouteDO);
                // This fix inability of MockDB to correctly resolve requests to collections of derived entites
                uow.RouteNodeRepository.Add(_subrouteDO);
                uow.RouteNodeRepository.Add(_routeDO);
                uow.SaveChanges();
            }

            _coreServer = terminalIntegrationTests.Fixtures.FixtureData.CreateCoreServer_ActivitiesController();

            var docuSignServerUrl = "http://" + FixtureData.TestTerminal_DocuSign_EndPoint + "/";
            _docuSignServer = terminalDocuSign.SelfHostFactory.CreateServer(docuSignServerUrl);

            var dockyardCoreServerUrl = "http://" + FixtureData.TestTerminal_Core_EndPoint + "/";
            _dockyardCoreServer = terminalFr8Core.SelfHostFactory.CreateServer(dockyardCoreServerUrl);

            var azureSqlServerServerUrl = "http://" + FixtureData.TestTerminal_AzureSqlServer_EndPoint + "/";
            _azureSqlServerServer = terminalAzure.SelfHostFactory.CreateServer(azureSqlServerServerUrl);

            _crateManager = ObjectFactory.GetInstance<ICrateManager>();

        }

        /// <summary>
        /// Remove _waitForDocuSignEventActivityTemplate instance from mock DB.
        /// Remove _testUserAccount instance from mock DB.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _coreServer.Dispose();
            _dockyardCoreServer.Dispose();
            _docuSignServer.Dispose();
            _azureSqlServerServer.Dispose();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var authToken = uow.AuthorizationTokenRepository.FindTokenById(_authToken.Id.ToString());

                if (authToken != null)
                {
                    uow.AuthorizationTokenRepository.Remove(authToken);
                }

                var curUser = uow.UserRepository.GetQuery()
                    .SingleOrDefault(x => x.Id == _testUserAccount.Id);
                if (curUser != null)
                {
                    uow.UserRepository.Remove(curUser);
                }

                var testIncomingDataActivityTemplate = uow.ActivityTemplateRepository
                    .GetByKey(_testIncomingDataActivityTemplate.Id);
                if (testIncomingDataActivityTemplate != null)
                {
                    uow.ActivityTemplateRepository.Remove(testIncomingDataActivityTemplate);
                }

                var waitForDocSignActivityTemplate = uow.ActivityTemplateRepository
                    .GetByKey(_waitForDocuSignEventActivityTemplate.Id);
                if (waitForDocSignActivityTemplate != null)
                {
                    uow.ActivityTemplateRepository.Remove(waitForDocSignActivityTemplate);
                }

                var writeToSqlServerActivityTemplate = uow.ActivityTemplateRepository
                    .GetByKey(_writeToSqlServerActivityTemplate.Id);
                if (writeToSqlServerActivityTemplate != null)
                {
                    uow.ActivityTemplateRepository.Remove(writeToSqlServerActivityTemplate);
                }

                var sendDocuSignEnvelopeActivityTemplate = uow.ActivityTemplateRepository
                      .GetByKey(_sendDocuSignEnvelopeActivityTemplate.Id);
                if (sendDocuSignEnvelopeActivityTemplate != null)
                {
                    uow.ActivityTemplateRepository.Remove(sendDocuSignEnvelopeActivityTemplate);
                }

                //                var actionList = uow.ActivityRepository.GetByKey(_actionList.Id);
                //                if (actionList != null)
                //                {
                //                    uow.ActivityRepository.Remove(actionList);
                //                }

                uow.SaveChanges();
            }
        }

        private ActionDTO CreateEmptyAction(ActivityTemplateDO activityTemplate)
        {
            var curActionController = CreateActionController();
            var curActionDO = FixtureData.TestAction_Blank();

            if (_subrouteDO.ChildNodes == null)
            {
                _subrouteDO.ChildNodes = new List<RouteNodeDO>();
                _subrouteDO.ChildNodes.Add(curActionDO);
            }

            if (activityTemplate != null)
            {
                curActionDO.ActivityTemplate = activityTemplate;
                curActionDO.ActivityTemplateId = activityTemplate.Id;
            }

            curActionDO.ParentRouteNode = _subrouteDO;
            curActionDO.ParentRouteNodeId = _subrouteDO.Id;

            var curActionDTO = Mapper.Map<ActionDTO>(curActionDO);

            curActionDTO.IsTempId = true;

            var result = curActionController.Save(curActionDTO)
                as OkNegotiatedContentResult<ActionDTO>;

            // Assert action was property saved.
            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.AreEqual(result.Content.ActivityTemplateId, curActionDTO.ActivityTemplateId);
            Assert.AreEqual(result.Content.CrateStorage, curActionDTO.CrateStorage);


            return result.Content;
        }

        private ActionDTO SaveAction(ActionDTO curActionDTO)
        {
            var curActionController = CreateActionController();

            var result = curActionController.Save(curActionDTO)
                as OkNegotiatedContentResult<ActionDTO>;

            // Assert action was property saved.
            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.AreEqual(result.Content.ActivityTemplateId, curActionDTO.ActivityTemplateId);

            return result.Content;
        }

        private async Task<CrateStorage> WaitForDocuSignEvent_ConfigureInitial(ActionDTO curActionDTO)
        {
            // Fill values as it would be on front-end.
            curActionDTO.ActivityTemplate = Mapper.Map<ActivityTemplateDTO>(_waitForDocuSignEventActivityTemplate);
            curActionDTO.ActivityTemplateId = _waitForDocuSignEventActivityTemplate.Id;
            curActionDTO.CrateStorage = new CrateStorageDTO();

            // Send initial configure request.
            var curActionController = CreateActionController();
            var actionDTO = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActionDTO>;



            // Assert initial configuration returned in CrateStorage.
            Assert.NotNull(actionDTO);
            Assert.NotNull(actionDTO.Content);
            Assert.NotNull(actionDTO.Content.CrateStorage);

            var storage = _crateManager.GetStorage(actionDTO.Content);

            Assert.AreEqual(4,storage.Count);
            Assert.True((storage.CratesOfType<StandardConfigurationControlsCM>().Any()));
            Assert.True(storage.CratesOfType<StandardDesignTimeFieldsCM>().Any(x => x.Label == "Available Templates"));

            FixActionNavProps(actionDTO.Content.Id);

            return storage;
        }

        // navigational properties in MockDB are not so navigational... 
        private void FixActionNavProps(Guid id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activity = uow.ActionRepository.GetByKey(id);

                activity.ParentRouteNode = (RouteNodeDO)uow.SubrouteRepository.GetByKey(activity.ParentRouteNodeId) ?? uow.ActionRepository.GetByKey(activity.ParentRouteNodeId);
                uow.SaveChanges();
            }
        }

        private void WaitForDocuSignEvent_SelectFirstTemplate(CrateStorage curCrateStorage)
        {
            // Fetch Available Template crate and parse StandardDesignTimeFieldsMS.
            var availableTemplatesCrate = curCrateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Single(x => x.Label == "Available Templates");

            var fieldsMS = availableTemplatesCrate.Content;

            // Fetch Configuration Controls crate and parse StandardConfigurationControlsMS
            var configurationControlsCrate = curCrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single(x => x.Label == "Configuration_Controls");
            var controlsMS = configurationControlsCrate.Content;

            controlsMS.Controls.OfType<RadioButtonGroup>().First().Radios.ForEach(r => r.Selected = false);

            // Modify value of Selected_DocuSign_Template field and push it back to crate,
            // exact same way we do on front-end.
            var docuSignTemplateControl =
                controlsMS.Controls.OfType<RadioButtonGroup>().First().Radios.Single(r => r.Name.Equals("template"));

            docuSignTemplateControl.Selected = true;
            docuSignTemplateControl.Controls[0].Value = fieldsMS.Fields.First().Value;
        }

        private async Task<CrateStorage> WaitForDocuSignEvent_ConfigureFollowUp(ActionDTO curActionDTO)
        {
            var curActionController = CreateActionController();

            var actionDTO = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActionDTO>;

            // Assert FollowUp Configure result.
            Assert.NotNull(actionDTO);
            Assert.NotNull(actionDTO.Content);
            Assert.NotNull(actionDTO.Content.CrateStorage);

            var storage = _crateManager.GetStorage(actionDTO.Content);

            Assert.AreEqual(4, storage.Count);//replace this with 3 when 1123 is fixed (Why 3?)
            Assert.True(storage.CratesOfType<StandardConfigurationControlsCM>().Any(x => x.Label == "Configuration_Controls"));
            Assert.True(storage.CratesOfType<StandardDesignTimeFieldsCM>().Any(x => x.Label == "Available Templates"));
            // Assert.True(storage.CratesOfType<StandardDesignTimeFieldsCM>().Any(x => x.Label == "DocuSignTemplateUserDefinedFields"));
            Assert.True(storage.CratesOfType<EventSubscriptionCM>().Any(x => x.Label == "Standard Event Subscriptions"));

            return storage;
        }

        private async Task<CrateStorage> TestIncomingData_ConfigureInitial(ActionDTO curActionDTO)
        {
            // Fill values as it would be on front-end.
            curActionDTO.ActivityTemplateId = _testIncomingDataActivityTemplate.Id;
            curActionDTO.CrateStorage = new CrateStorageDTO();

            // Send initial configure request.
            var curActionController = CreateActionController();
            var result = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActionDTO>;

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.CrateStorage);

            var storage = _crateManager.GetStorage(result.Content);

            Assert.AreEqual(storage.Count, 2);
            Assert.True(storage.CratesOfType<StandardConfigurationControlsCM>().Any(x => x.Label == "Configuration_Controls"));
            Assert.True(storage.CratesOfType<StandardDesignTimeFieldsCM>().Any(x => x.Label == "Queryable Criteria"));

            return storage;
        }

        private async Task<CrateStorage> WriteToSqlServer_ConfigureInitial(ActionDTO curActionDTO)
        {
            curActionDTO.ActivityTemplate = Mapper.Map<ActivityTemplateDTO>(_writeToSqlServerActivityTemplate);
            curActionDTO.ActivityTemplateId = _writeToSqlServerActivityTemplate.Id;
            curActionDTO.CrateStorage = new CrateStorageDTO();

            var curActionController = CreateActionController();
            var result = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActionDTO>;

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.CrateStorage);

            var storage = _crateManager.GetStorage(result.Content);

            Assert.AreEqual(storage.Count, 1);
            Assert.True(storage.CratesOfType<StandardConfigurationControlsCM>().Any(x => x.Label == "Configuration_Controls"));

            return storage;
        }

        private void WriteToSqlServer_InputConnectionString(CrateStorage curCrateStorage)
        {
            var controlsMS = curCrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            // Modify value of Selected_DocuSign_Template field and push it back to crate,
            // exact same way we do on front-end.
            var connectionStringControl = controlsMS.Controls.Single(x => x.Name == "connection_string");
            connectionStringControl.Value = "Server = tcp:s79ifqsqga.database.windows.net,1433; Database = demodb_health; User ID = alexeddodb@s79ifqsqga; Password = Thales89; Trusted_Connection = False; Encrypt = True; Connection Timeout = 30;";
        }

        private async Task<CrateStorage> WriteToSqlServer_ConfigureFollowUp(ActionDTO curActionDTO)
        {
            var curActionController = CreateActionController();

            var actionDTO = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActionDTO>;

            // Assert FollowUp Configure result.
            Assert.NotNull(actionDTO);
            Assert.NotNull(actionDTO.Content);

            var storage = _crateManager.GetStorage(actionDTO.Content);

            Assert.AreEqual(2, storage.Count);//replace this with 3 when 1123 is fixed
            Assert.True(storage.CratesOfType<StandardConfigurationControlsCM>().Any(x => x.Label == "Configuration_Controls"));
            Assert.True(storage.CratesOfType<StandardDesignTimeFieldsCM>().Any(x => x.Label == "Sql Table Columns"));

            return storage;
        }

        /// <summary>
        /// Test WaitForDocuSignEvent initial configuration.
        /// </summary>
        [Test, Ignore]
        public async Task TerminalIntegration_WaitForDocuSign_ConfigureInitial()
        {
            var savedActionDTO = CreateEmptyAction(_waitForDocuSignEventActivityTemplate);

            await WaitForDocuSignEvent_ConfigureInitial(savedActionDTO);
        }

        /// <summary>
        /// Test WaitForDocuSignEvent follow-up configuration.
        /// </summary>
        [Test, Ignore]
        public async Task TerminalIntegration_WaitForDocuSign_ConfigureFollowUp()
        {
            // Create blank WaitForDocuSignEventAction.
            var savedActionDTO = CreateEmptyAction(_waitForDocuSignEventActivityTemplate);

            // Call Configure Initial for WaitForDocuSignEvent action.
            var initCrateStorageDTO = await WaitForDocuSignEvent_ConfigureInitial(savedActionDTO);

            // Select first available DocuSign template.
            WaitForDocuSignEvent_SelectFirstTemplate(initCrateStorageDTO);

            using (var updater = _crateManager.UpdateStorage(savedActionDTO))
            {
                updater.CrateStorage = initCrateStorageDTO;
            }

            // Call Configure FollowUp for WaitForDocuSignEvent action.
            await WaitForDocuSignEvent_ConfigureFollowUp(savedActionDTO);
        }

        /// <summary>
        /// Test TestIncomingData initial configuration.
        /// </summary>
        [Test, Ignore]
        public async Task TerminalIntegration_TestIncomingData_ConfigureInitial()
        {
            // Create blank WaitForDocuSignEvent action.
            var waitForDocuSignEventAction = CreateEmptyAction(_waitForDocuSignEventActivityTemplate);

            // Call Configure Initial for WaitForDocuSignEvent action.
            var initWaitForDocuSignEventCS = await WaitForDocuSignEvent_ConfigureInitial(waitForDocuSignEventAction);

            // Select first available DocuSign template.
            WaitForDocuSignEvent_SelectFirstTemplate(initWaitForDocuSignEventCS);

            using (var updater = _crateManager.UpdateStorage(waitForDocuSignEventAction))
            {
                updater.CrateStorage = initWaitForDocuSignEventCS;
            }

            FixActionNavProps(waitForDocuSignEventAction.Id);

            // Call Configure FollowUp for WaitForDocuSignEvent action.
            await WaitForDocuSignEvent_ConfigureFollowUp(waitForDocuSignEventAction);

            FixActionNavProps(waitForDocuSignEventAction.Id);

            // Save WaitForDocuSignEvent action.
            SaveAction(waitForDocuSignEventAction);

            FixActionNavProps(waitForDocuSignEventAction.Id);

            // Create blank TestIncomingData action.
            var filterAction = CreateEmptyAction(_testIncomingDataActivityTemplate);


            // Call Configure Initial for TestIncomingData action.
            await TestIncomingData_ConfigureInitial(filterAction);

            FixActionNavProps(filterAction.Id);
        }

        /// <summary>
        /// Test WriteToSqlServer initial configuration.
        /// </summary>
        [Test, Ignore]
        public async Task TerminalIntegration_WriteToSqlServer_ConfigureInitial()
        {
            // Create blank WaitForDocuSignEvent action.
            var emptyAction = CreateEmptyAction(_writeToSqlServerActivityTemplate);

            // Call Configure Initial for WriteToSqlServer action.
            await WriteToSqlServer_ConfigureInitial(emptyAction);
        }

        /// <summary>
        /// Test WriteToSqlServer follow-up configuration.
        /// </summary>
        [Test, Ignore] //this is failing because it uses a password to connect to an azure sql server, and we changed that password for security reasons
        // we probably should replace this test with one that doesn't require that kind of access, or we need to set up a separate db server just for testing.
        public async Task TerminalIntegration_WriteToSqlServer_ConfigureFollowUp()
        {
            // Create blank WaitForDocuSignEventAction.
            var savedActionDTO = CreateEmptyAction(_writeToSqlServerActivityTemplate);

            // Call Configure Initial for WaitForDocuSignEvent action.
            var initCrateStorageDTO = await WriteToSqlServer_ConfigureInitial(savedActionDTO);

            // Select first available DocuSign template.
            WriteToSqlServer_InputConnectionString(initCrateStorageDTO);

            using (var updater = _crateManager.UpdateStorage(savedActionDTO))
            {
                updater.CrateStorage = initCrateStorageDTO;
            }

            FixActionNavProps(savedActionDTO.Id);

            // Call Configure FollowUp for WaitForDocuSignEvent action.
            await WriteToSqlServer_ConfigureFollowUp(savedActionDTO);
        }

        /// <summary>
        /// Create ActionController instance.
        /// </summary>
        public ActionsController CreateActionController()
        {
            return CreateController<ActionsController>(_testUserAccount.Id);
        }
    }
}
