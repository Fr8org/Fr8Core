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
        private PlanDO _planDO;
        private SubPlanDO _subPlanDO;
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

            _planDO = FixtureData.Plan_TerminalIntegration();
            _planDO.Fr8Account = _testUserAccount;
            System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(_testUserAccount.Id), new string[] { "User" });

            _subPlanDO = FixtureData.SubPlan_TerminalIntegration();
            _subPlanDO.ParentPlanNode = _planDO;



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

                uow.PlanRepository.Add(_planDO);
             
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

        private async Task <ActivityDTO> CreateEmptyActivity(ActivityTemplateDO activityTemplate)
        {
            var curActionController = CreateActivityController();
            var curActivityDO = FixtureData.TestActivity_Blank();

            if (_subPlanDO.ChildNodes == null)
            {
                _subPlanDO.ChildNodes = new List<PlanNodeDO>();
                _subPlanDO.ChildNodes.Add(curActivityDO);
            }

            if (activityTemplate != null)
            {
                curActivityDO.ActivityTemplate = activityTemplate;
                curActivityDO.ActivityTemplateId = activityTemplate.Id;
            }

            curActivityDO.ParentPlanNode = _subPlanDO;
            curActivityDO.ParentPlanNodeId = _subPlanDO.Id;

            var curActionDTO = Mapper.Map<ActivityDTO>(curActivityDO);
            var result = (await curActionController.Save(curActionDTO)) as OkNegotiatedContentResult<ActivityDTO>;

            // Assert action was property saved.
            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.AreEqual(result.Content.ActivityTemplate.Name, curActionDTO.ActivityTemplate.Name);
            Assert.AreEqual(result.Content.ActivityTemplate.Version, curActionDTO.ActivityTemplate.Version);
            Assert.AreEqual(result.Content.CrateStorage, curActionDTO.CrateStorage);


            return result.Content;
        }

        private async Task<ActivityDTO> SaveActivity(ActivityDTO curActivityDTO)
        {
            var curActionController = CreateActivityController();

            var result = await curActionController.Save(curActivityDTO) as OkNegotiatedContentResult<ActivityDTO>;

            // Assert action was property saved.
            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            //TODO bahadir check this -- is it ???
            Assert.AreEqual(result.Content.ActivityTemplate.Name, curActivityDTO.ActivityTemplate.Name);
            Assert.AreEqual(result.Content.ActivityTemplate.Version, curActivityDTO.ActivityTemplate.Version);
            Assert.AreEqual(result.Content.ActivityTemplate.Terminal.Name, curActivityDTO.ActivityTemplate.Terminal.Name);
            Assert.AreEqual(result.Content.ActivityTemplate.Terminal.Label, curActivityDTO.ActivityTemplate.Terminal.Label);
            return result.Content;
        }

        private async Task<ICrateStorage> WaitForDocuSignEvent_ConfigureInitial(ActivityDTO curActionDTO)
        {
            // Fill values as it would be on front-end.
            curActionDTO.ActivityTemplate = Mapper.Map<ActivityTemplateDTO>(_waitForDocuSignEventActivityTemplate);
            curActionDTO.CrateStorage = new CrateStorageDTO();

            // Send initial configure request.
            var curActionController = CreateActivityController();
            var activityDTO = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActivityDTO>;



            // Assert initial configuration returned in CrateStorage.
            Assert.NotNull(activityDTO);
            Assert.NotNull(activityDTO.Content);
            Assert.NotNull(activityDTO.Content.CrateStorage);

            var storage = _crateManager.GetStorage(activityDTO.Content);

            Assert.AreEqual(4,storage.Count);
            Assert.True((storage.CratesOfType<StandardConfigurationControlsCM>().Any()));
            Assert.True(storage.CratesOfType<FieldDescriptionsCM>().Any(x => x.Label == "Available Templates"));

           // FixActionNavProps(activityDTO.Content.Id);

            return storage;
        }

        // navigational properties in MockDB are not so navigational... 
//        private void FixActionNavProps(Guid id)
//        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                var activity = uow.ActivityRepository.GetByKey(id);
//
//                activity.ParentPlanNode = (PlanNodeDO)uow.SubPlanRepository.GetByKey(activity.ParentPlanNodeId) ?? uow.ActivityRepository.GetByKey(activity.ParentPlanNodeId);
//                uow.SaveChanges();
//            }
//        }

        private void WaitForDocuSignEvent_SelectFirstTemplate(ICrateStorage curCrateStorage)
        {
            // Fetch Available Template crate and parse StandardDesignTimeFieldsMS.
            var availableTemplatesCrate = curCrateStorage.CratesOfType<FieldDescriptionsCM>().Single(x => x.Label == "Available Templates");

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

        private async Task<ICrateStorage> WaitForDocuSignEvent_ConfigureFollowUp(ActivityDTO curActionDTO)
        {
            var curActionController = CreateActivityController();

            var activtyDTO = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActivityDTO>;

            // Assert FollowUp Configure result.
            Assert.NotNull(activtyDTO);
            Assert.NotNull(activtyDTO.Content);
            Assert.NotNull(activtyDTO.Content.CrateStorage);

            var storage = _crateManager.GetStorage(activtyDTO.Content);

            Assert.AreEqual(4, storage.Count);//replace this with 3 when 1123 is fixed (Why 3?)
            Assert.True(storage.CratesOfType<StandardConfigurationControlsCM>().Any(x => x.Label == "Configuration_Controls"));
            Assert.True(storage.CratesOfType<FieldDescriptionsCM>().Any(x => x.Label == "Available Templates"));
            // Assert.True(storage.CratesOfType<FieldDescriptionsCM>().Any(x => x.Label == "DocuSignTemplateUserDefinedFields"));
            Assert.True(storage.CratesOfType<EventSubscriptionCM>().Any(x => x.Label == "Standard Event Subscriptions"));

            return storage;
        }

        private async Task<ICrateStorage> TestIncomingData_ConfigureInitial(ActivityDTO curActionDTO)
        {
            // Fill values as it would be on front-end.
            curActionDTO.CrateStorage = new CrateStorageDTO();

            // Send initial configure request.
            var curActionController = CreateActivityController();
            var result = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActivityDTO>;

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.CrateStorage);

            var storage = _crateManager.GetStorage(result.Content);

            Assert.AreEqual(storage.Count, 2);
            Assert.True(storage.CratesOfType<StandardConfigurationControlsCM>().Any(x => x.Label == "Configuration_Controls"));
            Assert.True(storage.CratesOfType<FieldDescriptionsCM>().Any(x => x.Label == "Queryable Criteria"));

            return storage;
        }

        private async Task<ICrateStorage> WriteToSqlServer_ConfigureInitial(ActivityDTO curActionDTO)
        {
            curActionDTO.ActivityTemplate = Mapper.Map<ActivityTemplateDTO>(_writeToSqlServerActivityTemplate);
            curActionDTO.CrateStorage = new CrateStorageDTO();

            var curActionController = CreateActivityController();
            var result = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActivityDTO>;

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.CrateStorage);

            var storage = _crateManager.GetStorage(result.Content);

            Assert.AreEqual(storage.Count, 1);
            Assert.True(storage.CratesOfType<StandardConfigurationControlsCM>().Any(x => x.Label == "Configuration_Controls"));

            return storage;
        }

        private void WriteToSqlServer_InputConnectionString(ICrateStorage curCrateStorage)
        {
            var controlsMS = curCrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            // Modify value of Selected_DocuSign_Template field and push it back to crate,
            // exact same way we do on front-end.
            var connectionStringControl = controlsMS.Controls.Single(x => x.Name == "connection_string");
            connectionStringControl.Value = "Server = tcp:s79ifqsqga.database.windows.net,1433; Database = demodb_health; User ID = alexeddodb@s79ifqsqga; Password = Thales89; Trusted_Connection = False; Encrypt = True; Connection Timeout = 30;";
        }

        private async Task<ICrateStorage> WriteToSqlServer_ConfigureFollowUp(ActivityDTO curActionDTO)
        {
            var curActionController = CreateActivityController();

            var activityDTO = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActivityDTO>;

            // Assert FollowUp Configure result.
            Assert.NotNull(activityDTO);
            Assert.NotNull(activityDTO.Content);

            var storage = _crateManager.GetStorage(activityDTO.Content);

            Assert.AreEqual(2, storage.Count);//replace this with 3 when 1123 is fixed
            Assert.True(storage.CratesOfType<StandardConfigurationControlsCM>().Any(x => x.Label == "Configuration_Controls"));
            Assert.True(storage.CratesOfType<FieldDescriptionsCM>().Any(x => x.Label == "Sql Table Columns"));

            return storage;
        }

        /// <summary>
        /// Test WaitForDocuSignEvent initial configuration.
        /// </summary>
        [Test, Ignore]
        public async Task TerminalIntegration_WaitForDocuSign_ConfigureInitial()
        {
            var savedActionDTO = await CreateEmptyActivity(_waitForDocuSignEventActivityTemplate);

            await WaitForDocuSignEvent_ConfigureInitial(savedActionDTO);
        }

        /// <summary>
        /// Test WaitForDocuSignEvent follow-up configuration.
        /// </summary>
        [Test, Ignore]
        public async Task TerminalIntegration_WaitForDocuSign_ConfigureFollowUp()
        {
            // Create blank WaitForDocuSignEventAction.
            var savedActionDTO = await CreateEmptyActivity(_waitForDocuSignEventActivityTemplate);

            // Call Configure Initial for WaitForDocuSignEvent action.
            var initCrateStorageDTO = await WaitForDocuSignEvent_ConfigureInitial(savedActionDTO);

            // Select first available DocuSign template.
            WaitForDocuSignEvent_SelectFirstTemplate(initCrateStorageDTO);

            using (var crateStorage = _crateManager.GetUpdatableStorage(savedActionDTO))
            {
                crateStorage.Replace(initCrateStorageDTO);
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
            var waitForDocuSignEventAction = await CreateEmptyActivity(_waitForDocuSignEventActivityTemplate);

            // Call Configure Initial for WaitForDocuSignEvent action.
            var initWaitForDocuSignEventCS = await WaitForDocuSignEvent_ConfigureInitial(waitForDocuSignEventAction);

            // Select first available DocuSign template.
            WaitForDocuSignEvent_SelectFirstTemplate(initWaitForDocuSignEventCS);

            using (var crateStorage = _crateManager.GetUpdatableStorage(waitForDocuSignEventAction))
            {
                crateStorage.Replace(initWaitForDocuSignEventCS);
            }

            //FixActionNavProps(waitForDocuSignEventAction.Id);

            // Call Configure FollowUp for WaitForDocuSignEvent action.
            await WaitForDocuSignEvent_ConfigureFollowUp(waitForDocuSignEventAction);

         //   FixActionNavProps(waitForDocuSignEventAction.Id);

            // Save WaitForDocuSignEvent action.
            SaveActivity(waitForDocuSignEventAction);

        //    FixActionNavProps(waitForDocuSignEventAction.Id);

            // Create blank TestIncomingData action.
            var filterAction = await CreateEmptyActivity(_testIncomingDataActivityTemplate);


            // Call Configure Initial for TestIncomingData action.
            await TestIncomingData_ConfigureInitial(filterAction);

           // FixActionNavProps(filterAction.Id);
        }

        /// <summary>
        /// Test WriteToSqlServer initial configuration.
        /// </summary>
        [Test, Ignore]
        public async Task TerminalIntegration_WriteToSqlServer_ConfigureInitial()
        {
            // Create blank WaitForDocuSignEvent action.
            var emptyAction = await CreateEmptyActivity(_writeToSqlServerActivityTemplate);

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
            var savedActionDTO = await CreateEmptyActivity(_writeToSqlServerActivityTemplate);

            // Call Configure Initial for WaitForDocuSignEvent action.
            var initCrateStorageDTO = await WriteToSqlServer_ConfigureInitial(savedActionDTO);

            // Select first available DocuSign template.
            WriteToSqlServer_InputConnectionString(initCrateStorageDTO);

            using (var crateStorage = _crateManager.GetUpdatableStorage(savedActionDTO))
            {
                crateStorage.Replace(initCrateStorageDTO);
            }

          //  FixActionNavProps(savedActionDTO.Id);

            // Call Configure FollowUp for WaitForDocuSignEvent action.
            await WriteToSqlServer_ConfigureFollowUp(savedActionDTO);
        }

        /// <summary>
        /// Create ActionController instance.
        /// </summary>
        public ActivitiesController CreateActivityController()
        {
            return CreateController<ActivitiesController>(_testUserAccount.Id);
        }
    }
}
