using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using AutoMapper;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Plugin;
using NUnit.Framework;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using pluginAzureSqlServer;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using UtilitiesTesting;

namespace pluginIntegrationTests
{
    [TestFixture]
    public class PluginIntegrationTests : BaseTest
    {
        private IDisposable _docuSignServer;
        private IDisposable _dockyardCoreServer;
        private IDisposable _azureSqlServerServer;

        private DockyardAccountDO _testUserAccount;
        private ActionListDO _actionList;
        private ActivityTemplateDO _waitForDocuSignEventActivityTemplate;
        private ActivityTemplateDO _filterUsingRunTimeDataActivityTemplate;
        private ActivityTemplateDO _writeToSqlServerActivityTemplate;

        /// <summary>
        /// Create _testUserAccount instance and store it in mock DB.
        /// Create _waitForDocuSignEventActivityTemplate instance and store it in mock DB.
        /// </summary>
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            // these are integration tests, we are using a real transmitter
            ObjectFactory.Configure(c => c.For<IPluginTransmitter>().Use<PluginTransmitter>());

            _testUserAccount = FixtureData.TestUser1();

            _actionList = FixtureData.TestActionList_ImmediateActions();

            _waitForDocuSignEventActivityTemplate =
                FixtureData.TestActivityTemplateDO_WaitForDocuSignEvent();
            
            _filterUsingRunTimeDataActivityTemplate =
                FixtureData.TestActivityTemplateDO_FilterUsingRunTimeData();

            _writeToSqlServerActivityTemplate =
                FixtureData.TestActivityTemplateDO_WriteToSqlServer();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityRepository.Add(_actionList);
                uow.ActivityTemplateRepository.Add(_waitForDocuSignEventActivityTemplate);
                uow.ActivityTemplateRepository.Add(_filterUsingRunTimeDataActivityTemplate);
                uow.ActivityTemplateRepository.Add(_writeToSqlServerActivityTemplate);
                uow.UserRepository.Add(_testUserAccount);

                uow.SaveChanges();
            }

            var docuSignServerUrl = "http://" + FixtureData.TestPlugin_DocuSign_EndPoint + "/";
            _docuSignServer = pluginDocuSign.SelfHostFactory.CreateServer(docuSignServerUrl);

            var dockyardCoreServerUrl = "http://" + FixtureData.TestPlugin_Core_EndPoint + "/";
            _dockyardCoreServer = pluginDockyardCore.SelfHostFactory.CreateServer(dockyardCoreServerUrl);

            var azureSqlServerServerUrl = "http://" + FixtureData.TestPlugin_AzureSqlServer_EndPoint + "/";
            _azureSqlServerServer = pluginAzureSqlServer.SelfHostFactory.CreateServer(azureSqlServerServerUrl);
        }

        /// <summary>
        /// Remove _waitForDocuSignEventActivityTemplate instance from mock DB.
        /// Remove _testUserAccount instance from mock DB.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _dockyardCoreServer.Dispose();
            _docuSignServer.Dispose();
            _azureSqlServerServer.Dispose();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUser = uow.UserRepository.GetQuery()
                    .SingleOrDefault(x => x.Id == _testUserAccount.Id);
                if (curUser != null)
                {
                    uow.UserRepository.Remove(curUser);
                }

                var filterUsingRunTimeDataActivityTemplate = uow.ActivityTemplateRepository
                    .GetByKey(_filterUsingRunTimeDataActivityTemplate.Id);
                if (filterUsingRunTimeDataActivityTemplate != null)
                {
                    uow.ActivityTemplateRepository.Remove(filterUsingRunTimeDataActivityTemplate);
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

                var actionList = uow.ActivityRepository
                    .GetByKey(_actionList.Id);
                if (actionList != null)
                {
                    uow.ActivityRepository.Remove(actionList);
                }

                uow.SaveChanges();
            }
        }

        private ActionDTO CreateEmptyAction(ActivityTemplateDO activityTemplate)
        {
            var curActionController = CreateActionController();
            var curActionDO = FixtureData.TestAction_Blank();

            if (_actionList.Activities == null)
            {
                _actionList.Activities = new List<ActivityDO>();
                _actionList.Activities.Add(curActionDO);
            }

            if (activityTemplate != null)
            {
                curActionDO.ActivityTemplate = activityTemplate;
                curActionDO.ActivityTemplateId = activityTemplate.Id;
            }

            curActionDO.ParentActivity = _actionList;
            curActionDO.ParentActivityId = _actionList.Id;

            var curActionDTO = Mapper.Map<ActionDTO>(curActionDO);

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

        private async Task<CrateStorageDTO> WaitForDocuSignEvent_ConfigureInitial(ActionDTO curActionDTO)
        {
            // Fill values as it would be on front-end.
            curActionDTO.ActivityTemplate = Mapper.Map<ActivityTemplateDTO>(_waitForDocuSignEventActivityTemplate);
            curActionDTO.ActivityTemplateId = _waitForDocuSignEventActivityTemplate.Id;
            curActionDTO.CrateStorage = new CrateStorageDTO();

            // Send initial configure request.
            var curActionController = CreateActionController();
            var  actionDTO = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActionDTO>;

            

            // Assert initial configuration returned in CrateStorage.
            Assert.NotNull(actionDTO);
            Assert.NotNull(actionDTO.Content);
            Assert.NotNull(actionDTO.Content.CrateStorage.CrateDTO);
            Assert.AreEqual(actionDTO.Content.CrateStorage.CrateDTO.Count, 2);
            Assert.True((actionDTO.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME)));
            Assert.True(actionDTO.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Available Templates" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));

            return actionDTO.Content.CrateStorage;
        }

        private void WaitForDocuSignEvent_SelectFirstTemplate(CrateStorageDTO curCrateStorage)
        {
            // Fetch Available Template crate and parse StandardDesignTimeFieldsMS.
            var availableTemplatesCrate = curCrateStorage.CrateDTO
                .Single(x => x.Label == "Available Templates" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME);

            var fieldsMS = JsonConvert.DeserializeObject<StandardDesignTimeFieldsMS>(
                availableTemplatesCrate.Contents);

            // Fetch Configuration Controls crate and parse StandardConfigurationControlsMS
            var configurationControlsCrate = curCrateStorage.CrateDTO
                .Single(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);

            var controlsMS = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(
                configurationControlsCrate.Contents);

            // Modify value of Selected_DocuSign_Template field and push it back to crate,
            // exact same way we do on front-end.
            var docuSignTemplateControl = controlsMS.Controls.Single(x => x.Name == "Selected_DocuSign_Template");
            docuSignTemplateControl.Value = fieldsMS.Fields.First().Value;

            configurationControlsCrate.Contents = JsonConvert.SerializeObject(controlsMS);
        }

        private async Task<CrateStorageDTO> WaitForDocuSignEvent_ConfigureFollowUp(ActionDTO curActionDTO)
        {
            var curActionController = CreateActionController();

            var actionDTO = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActionDTO>;

            // Assert FollowUp Configure result.
            Assert.NotNull(actionDTO);
            Assert.NotNull(actionDTO.Content);
            Assert.NotNull(actionDTO.Content.CrateStorage.CrateDTO);
            Assert.AreEqual(3, actionDTO.Content.CrateStorage.CrateDTO.Count);//replace this with 3 when 1123 is fixed
            Assert.True(actionDTO.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
            //Assert.True(result.Content.CrateDTO   //uncomment this when 1123 is fixed
              //  .Any(x => x.Label == "Available Templates" && x.ManifestType == "Standard Design-Time Fields"));
            //Assert.True(actionDTO.Content.CrateStorage.CrateDTO
            //    .Any(x => x.Label == "DocuSignTemplateUserDefinedFields" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));

            Assert.True(actionDTO.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Standard Event Subscriptions" && x.ManifestType == CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_NAME));

            return actionDTO.Content.CrateStorage;
        }

        private async Task<CrateStorageDTO> FilterUsingRunTimeData_ConfigureInitial(ActionDTO curActionDTO)
        {
            // Fill values as it would be on front-end.
            curActionDTO.ActivityTemplateId = _filterUsingRunTimeDataActivityTemplate.Id;
            curActionDTO.CrateStorage = new CrateStorageDTO();

            // Send initial configure request.
            var curActionController = CreateActionController();
            var result = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActionDTO>;

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.CrateStorage.CrateDTO);
            Assert.AreEqual(result.Content.CrateStorage.CrateDTO.Count, 2);
            Assert.True(result.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
            Assert.True(result.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Queryable Criteria" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));

            return result.Content.CrateStorage;
        }

        private async Task<CrateStorageDTO> WriteToSqlServer_ConfigureInitial(ActionDTO curActionDTO)
        {
            curActionDTO.ActivityTemplate = Mapper.Map<ActivityTemplateDTO>(_writeToSqlServerActivityTemplate);
            curActionDTO.ActivityTemplateId = _writeToSqlServerActivityTemplate.Id;
            curActionDTO.CrateStorage = new CrateStorageDTO();
            var curActionController = CreateActionController();
            var result = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActionDTO>;

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.CrateStorage.CrateDTO);
            Assert.AreEqual(result.Content.CrateStorage.CrateDTO.Count, 1);
            Assert.True(result.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));

            return result.Content.CrateStorage;
        }

        private void WriteToSqlServer_InputConnectionString(CrateStorageDTO curCrateStorage)
        {
            // Fetch Configuration Controls crate and parse StandardConfigurationControlsMS
            var configurationControlsCrate = curCrateStorage.CrateDTO
                .Single(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);

            var controlsMS = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(
                configurationControlsCrate.Contents);

            // Modify value of Selected_DocuSign_Template field and push it back to crate,
            // exact same way we do on front-end.
            var connectionStringControl = controlsMS.Controls.Single(x => x.Name == "connection_string");
            connectionStringControl.Value = "Server = tcp:s79ifqsqga.database.windows.net,1433; Database = demodb_health; User ID = alexeddodb@s79ifqsqga; Password = Thales89; Trusted_Connection = False; Encrypt = True; Connection Timeout = 30;";

            configurationControlsCrate.Contents = JsonConvert.SerializeObject(controlsMS);
        }

        private async Task<CrateStorageDTO> WriteToSqlServer_ConfigureFollowUp(ActionDTO curActionDTO)
        {
            var curActionController = CreateActionController();

            var actionDTO = await curActionController.Configure(curActionDTO) as OkNegotiatedContentResult<ActionDTO>;

            // Assert FollowUp Configure result.
            Assert.NotNull(actionDTO);
            Assert.NotNull(actionDTO.Content);
            Assert.NotNull(actionDTO.Content.CrateStorage.CrateDTO);
            Assert.AreEqual(2, actionDTO.Content.CrateStorage.CrateDTO.Count);//replace this with 3 when 1123 is fixed
            Assert.True(actionDTO.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
            Assert.True(actionDTO.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Sql Table Columns" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));

            return actionDTO.Content.CrateStorage;
        }

        /// <summary>
        /// Test WaitForDocuSignEvent initial configuration.
        /// </summary>
        [Test]
        public async Task PluginIntegration_WaitForDocuSign_ConfigureInitial()
        {
            var savedActionDTO = CreateEmptyAction(_waitForDocuSignEventActivityTemplate);
            await WaitForDocuSignEvent_ConfigureInitial(savedActionDTO);
        }

        /// <summary>
        /// Test WaitForDocuSignEvent follow-up configuration.
        /// </summary>
        [Test]
        public async Task PluginIntegration_WaitForDocuSign_ConfigureFollowUp()
        {
            // Create blank WaitForDocuSignEventAction.
            var savedActionDTO = CreateEmptyAction(_waitForDocuSignEventActivityTemplate);
            
            // Call Configure Initial for WaitForDocuSignEvent action.
            var initCrateStorageDTO = await WaitForDocuSignEvent_ConfigureInitial(savedActionDTO);
            
            // Select first available DocuSign template.
            WaitForDocuSignEvent_SelectFirstTemplate(initCrateStorageDTO);
            savedActionDTO.CrateStorage = initCrateStorageDTO;

            // Call Configure FollowUp for WaitForDocuSignEvent action.
            await WaitForDocuSignEvent_ConfigureFollowUp(savedActionDTO);
        }

        /// <summary>
        /// Test FilterUsingRunTimeData initial configuration.
        /// </summary>
        [Test]
        public async Task PluginIntegration_FilterUsingRunTimeData_ConfigureInitial()
        {
            // Create blank WaitForDocuSignEvent action.
            var waitForDocuSignEventAction = CreateEmptyAction(_waitForDocuSignEventActivityTemplate);

            // Call Configure Initial for WaitForDocuSignEvent action.
            var initWaitForDocuSignEventCS = await WaitForDocuSignEvent_ConfigureInitial(waitForDocuSignEventAction);

            // Select first available DocuSign template.
            WaitForDocuSignEvent_SelectFirstTemplate(initWaitForDocuSignEventCS);
            waitForDocuSignEventAction.CrateStorage = initWaitForDocuSignEventCS;

            // Call Configure FollowUp for WaitForDocuSignEvent action.
            await WaitForDocuSignEvent_ConfigureFollowUp(waitForDocuSignEventAction);

            // Save WaitForDocuSignEvent action.
            SaveAction(waitForDocuSignEventAction);

            // Create blank FilterUsingRunTimeData action.
            var filterAction = CreateEmptyAction(_filterUsingRunTimeDataActivityTemplate);

            // Call Configure Initial for FilterUsingRunTimeData action.
            await FilterUsingRunTimeData_ConfigureInitial(filterAction);
        }

        /// <summary>
        /// Test WriteToSqlServer initial configuration.
        /// </summary>
        [Test]
        public async Task PluginIntegration_WriteToSqlServer_ConfigureInitial()
        {
            // Create blank WaitForDocuSignEvent action.
            var emptyAction = CreateEmptyAction(_writeToSqlServerActivityTemplate);

            // Call Configure Initial for WriteToSqlServer action.
            await WriteToSqlServer_ConfigureInitial(emptyAction);
        }

        /// <summary>
        /// Test WriteToSqlServer follow-up configuration.
        /// </summary>
        [Test]
        public async Task PluginIntegration_WriteToSqlServer_ConfigureFollowUp()
        {
            // Create blank WaitForDocuSignEventAction.
            var savedActionDTO = CreateEmptyAction(_writeToSqlServerActivityTemplate);

            // Call Configure Initial for WaitForDocuSignEvent action.
            var initCrateStorageDTO = await WriteToSqlServer_ConfigureInitial(savedActionDTO);

            // Select first available DocuSign template.
            WriteToSqlServer_InputConnectionString(initCrateStorageDTO);
            savedActionDTO.CrateStorage = initCrateStorageDTO;

            // Call Configure FollowUp for WaitForDocuSignEvent action.
            await WriteToSqlServer_ConfigureFollowUp(savedActionDTO);
        }

        /// <summary>
        /// Create ActionController instance.
        /// </summary>
        public ActionController CreateActionController()
        {
            return CreateController<ActionController>(_testUserAccount.Id);
        }
    }
}
