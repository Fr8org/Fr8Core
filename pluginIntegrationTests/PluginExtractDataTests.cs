using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using AutoMapper;
using Core.Interfaces;
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
using pluginExcel.Actions;
using System.Threading.Tasks;

namespace PluginExtractDataTests
{
    [TestFixture]
    public class PluginExtractDataTests : BaseTest
    {
        private IDisposable _docuSignServer;
        private IDisposable _dockyardCoreServer;
        private IDisposable _azureSqlServerServer;
        private IDisposable _extractDataServer;
        private IDisposable _fileServer;

        private DockyardAccountDO _testUserAccount;
        private ActionListDO _actionList;
        private ActivityTemplateDO _waitForDocuSignEventActivityTemplate;
        private ActivityTemplateDO _filterUsingRunTimeDataActivityTemplate;
        private ActivityTemplateDO _writeToSqlServerActivityTemplate;
        private ActivityTemplateDO _extractDataActivityTemplate;

        /// <summary>
        /// Create _testUserAccount instance and store it in mock DB.
        /// Create _waitForDocuSignEventActivityTemplate instance and store it in mock DB.
        /// </summary>
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _testUserAccount = FixtureData.TestUser1();

            _actionList = FixtureData.TestActionList_ImmediateActions();

            _waitForDocuSignEventActivityTemplate =
                FixtureData.TestActivityTemplateDO_WaitForDocuSignEvent();

            _filterUsingRunTimeDataActivityTemplate =
                FixtureData.TestActivityTemplateDO_FilterUsingRunTimeData();

            _writeToSqlServerActivityTemplate =
                FixtureData.TestActivityTemplateDO_WriteToSqlServer();

            _extractDataActivityTemplate =
                            FixtureData.TestActivityTemplateDO_ExtractData();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityRepository.Add(_actionList);
                uow.ActivityTemplateRepository.Add(_extractDataActivityTemplate);
                uow.UserRepository.Add(_testUserAccount);

                uow.SaveChanges();
            }

            var extractDatarServerUrl = "http://" + FixtureData.TestPlugin_ExtractData_EndPoint + "/";
            _extractDataServer = pluginExcel.SelfHostFactory.CreateServer(extractDatarServerUrl);

            var fileServerUrl = "http://" + FixtureData.TestPlugin_FileServer_EndPoint + "/";
            _fileServer = Web.Startup.CreateServer(fileServerUrl);

        }

        /// <summary>
        /// Remove _waitForDocuSignEventActivityTemplate instance from mock DB.
        /// Remove _testUserAccount instance from mock DB.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            //_dockyardCoreServer.Dispose();
            //_docuSignServer.Dispose();
            //_azureSqlServerServer.Dispose();
            _extractDataServer.Dispose();

            //using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            //{
            //    var curUser = uow.UserRepository.GetQuery()
            //        .SingleOrDefault(x => x.Id == _testUserAccount.Id);
            //    if (curUser != null)
            //    {
            //        uow.UserRepository.Remove(curUser);
            //    }

            //    var filterUsingRunTimeDataActivityTemplate = uow.ActivityTemplateRepository
            //        .GetByKey(_filterUsingRunTimeDataActivityTemplate.Id);
            //    if (filterUsingRunTimeDataActivityTemplate != null)
            //    {
            //        uow.ActivityTemplateRepository.Remove(filterUsingRunTimeDataActivityTemplate);
            //    }

            //    var waitForDocSignActivityTemplate = uow.ActivityTemplateRepository
            //        .GetByKey(_waitForDocuSignEventActivityTemplate.Id);
            //    if (waitForDocSignActivityTemplate != null)
            //    {
            //        uow.ActivityTemplateRepository.Remove(waitForDocSignActivityTemplate);
            //    }

            //    var writeToSqlServerActivityTemplate = uow.ActivityTemplateRepository
            //        .GetByKey(_writeToSqlServerActivityTemplate.Id);
            //    if (writeToSqlServerActivityTemplate != null)
            //    {
            //        uow.ActivityTemplateRepository.Remove(writeToSqlServerActivityTemplate);
            //    }

            //    var actionList = uow.ActivityRepository
            //        .GetByKey(_actionList.Id);
            //    if (actionList != null)
            //    {
            //        uow.ActivityRepository.Remove(actionList);
            //    }

            //    uow.SaveChanges();
            //}
        }

        private ActionDTO CreateEmptyAction()
        {
            var curActionController = CreateActionController();
            var curActionDO = FixtureData.TestAction_Blank();
            curActionDO.Ordering = 2;
            //var fileHandleActivity = new ActivityDO()
            //{
            //    Activities = new List<ActivityDO>(),
            //    Ordering = 0,
            //    Id = 1,
            //};
            //fileHandleActivity.Activities.Add(new ActionDO()
            //    {
            //        Name = "File Picker Action",
            //        CrateStorage = CreateFileHandleMSCrateDTO().Contents,
            //        Ordering = 0,
            //    });
            var filePickerActionDO = new ActionDO()
                {
                    Name = "File Picker Action",
                    CrateStorage = CreateFileHandleMSCrateDTO().Contents,
                    Ordering = 1,
                };

            if (_actionList.Activities == null)
            {
                _actionList.Activities = new List<ActivityDO>();
                //_actionList.Activities.Add(fileHandleActivity);
                _actionList.Activities.Add(filePickerActionDO);
                _actionList.Activities.Add(curActionDO);
            }

            filePickerActionDO.ParentActivity = _actionList;
            filePickerActionDO.ParentActivityId = _actionList.Id;

            curActionDO.ParentActivity = _actionList;
            curActionDO.ParentActivityId = _actionList.Id;

            var curActionDTO = Mapper.Map<ActionDTO>(curActionDO);
            var filePickerActionDTO = Mapper.Map<ActionDTO>(filePickerActionDO);

            var result = curActionController.Save(filePickerActionDTO)
                as OkNegotiatedContentResult<ActionDTO>;
            result = curActionController.Save(curActionDTO)
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

        private void ExtractData_SelectExcelFile(CrateStorageDTO curCrateStorage)
        {
            // Fetch Configuration Controls crate and parse StandardConfigurationControlsMS
            var configurationControlsCrate = curCrateStorage.CrateDTO
                .Single(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);

            var controlsMS = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(
                configurationControlsCrate.Contents);

            var fileControl = controlsMS.Controls.Single(x => x.Name == "select_file");
            fileControl.Value = @"..\..\Sample Files\SampleFile1.xlsx";

            configurationControlsCrate.Contents = JsonConvert.SerializeObject(controlsMS);
        }

        private CrateDTO CreateFileHandleMSCrateDTO()
        {
            StandardFileHandleMS fileHandle = new StandardFileHandleMS()
            {
                DockyardStorageUrl = @"..\..\Sample Files\",
                Filename = "SampleFile1.xlsx",
            };

            return new CrateDTO()
            {
                Contents = JsonConvert.SerializeObject(fileHandle),
                Label = "file_handle",
                ManifestType = CrateManifests.STANDARD_FILE_HANDLE_MANIFEST_NAME,
                ManifestId = CrateManifests.STANDARD_FILE_HANDLE_MANIFEST_ID
            };
        }

        private CrateStorageDTO ExtractData_ConfigureInitial(ActionDTO curActionDTO)
        {
            curActionDTO.ActivityTemplateId = _extractDataActivityTemplate.Id;
            if(curActionDTO.CrateStorage == null) 
                curActionDTO.CrateStorage = new CrateStorageDTO();
            var curActionController = CreateActionController();
            var result = curActionController.Configure(curActionDTO)
                as OkNegotiatedContentResult<ActionDTO>;

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.NotNull(result.Content.CrateStorage.CrateDTO);
            Assert.AreEqual(result.Content.CrateStorage.CrateDTO.Count, 2);
            Assert.True(result.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
            Assert.True(result.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Select Excel File" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));

            return result.Content.CrateStorage;
        }

        private CrateStorageDTO ExtractData_ConfigureFollowUp(ActionDTO curActionDTO)
        {
            var curActionController = CreateActionController();

            //curActionDTO.CrateStorage = ExtractData_ConfigureInitial(curActionDTO);
            ///curActionDTO.CrateStorage.CrateDTO.Add(new CrateDTO() {});
            var actionDTO = curActionController.Configure(curActionDTO)
                as OkNegotiatedContentResult<ActionDTO>;

            // Assert FollowUp Configure result.
            Assert.NotNull(actionDTO);
            Assert.NotNull(actionDTO.Content);
            Assert.NotNull(actionDTO.Content.CrateStorage.CrateDTO);
            Assert.True(actionDTO.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
            Assert.True(actionDTO.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Select Excel File" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));
            Assert.True(actionDTO.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Spreadsheet Column Headers" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));
            Assert.True(actionDTO.Content.CrateStorage.CrateDTO
                .Any(x => x.Label == "Excel Payload Rows" && x.ManifestType == CrateManifests.STANDARD_TABLE_DATA_MANIFEST_NAME));

            return actionDTO.Content.CrateStorage;
        }

        private async Task<CrateStorageDTO> ExtractData_Execute(ActionDTO curActionDTO)
        {
            var curActionController = CreateActionController();

            var extractDataAction = new ExtractData_v1();
            var actionDTO = extractDataAction.Configure(curActionDTO);

            var executeResult = await extractDataAction.Execute(actionDTO);
            
            // Assert FollowUp Configure result.
            Assert.NotNull(executeResult);
            Assert.NotNull(executeResult);
            Assert.NotNull(executeResult.CrateStorage.CrateDTO);
            Assert.True(executeResult.CrateStorage.CrateDTO
                .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
            Assert.True(executeResult.CrateStorage.CrateDTO
                .Any(x => x.Label == "Select Excel File" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));
            Assert.True(executeResult.CrateStorage.CrateDTO
                .Any(x => x.Label == "Spreadsheet Column Headers" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));
            Assert.True(executeResult.CrateStorage.CrateDTO
                .Any(x => x.Label == "Excel Payload Rows" && x.ManifestType == CrateManifests.STANDARD_TABLE_DATA_MANIFEST_NAME));
            Assert.True(executeResult.CrateStorage.CrateDTO
                .Any(x => x.Label == "ExcelTableRow" && x.ManifestType == CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME));

            return executeResult.CrateStorage;
        }

        /// <summary>
        /// Test WriteToSqlServer initial configuration.
        /// </summary>
        [Test]
        public void PluginIntegration_ExtractData_ConfigureInitial()
        {
            // Create blank ExtractData action.
            var emptyAction = CreateEmptyAction();

            // Call Configure Initial for WriteToSqlServer action.
            ExtractData_ConfigureInitial(emptyAction);
        }

        /// <summary>
        /// Test WriteToSqlServer follow-up configuration.
        /// </summary>
        [Test]
        public void PluginIntegration_ExtractData_ConfigureFollowUp()
        {
            // Create blank WaitForDocuSignEventAction.
            var savedActionDTO = CreateEmptyAction();

            // Call Configure Initial for ExtractData action.
            var initCrateStorageDTO = ExtractData_ConfigureInitial(savedActionDTO);

            // Select first available DocuSign template.
            ExtractData_SelectExcelFile(initCrateStorageDTO);
            savedActionDTO.CrateStorage = initCrateStorageDTO;

            // Call Configure FollowUp for ExtractData action.
            ExtractData_ConfigureFollowUp(savedActionDTO);
        }

        /// <summary>
        /// Test ExtractData follow-up configuration.
        /// </summary>
        [Test]
        public async void PluginIntegration_ExtractData_ExecuteWithStanardTableData()
        {
            // Create blank ExtractDataEvent.
            var savedActionDTO = CreateEmptyAction();
            
            // Call Configure Initial for ExtractData action.
            var initCrateStorageDTO = ExtractData_ConfigureInitial(savedActionDTO);

            // Select first available DocuSign template.
            ExtractData_SelectExcelFile(initCrateStorageDTO);
            savedActionDTO.CrateStorage = initCrateStorageDTO;

            // Call Configure FollowUp for ExtractData action.
            savedActionDTO.CrateStorage = ExtractData_ConfigureFollowUp(savedActionDTO);

            await ExtractData_Execute(savedActionDTO);
        }

        /// <summary>
        /// Test ExtractData follow-up configuration.
        /// </summary>
        [Test, Ignore]
        public async void PluginIntegration_ExtractData_ExecuteWithoutStanardTableData()
        {
            // Create blank ExtractDataEvent.
            var savedActionDTO = CreateEmptyAction();
            var crateDTO = CreateFileHandleMSCrateDTO();

            // Call Configure Initial for ExtractData action.
            var initCrateStorageDTO = ExtractData_ConfigureInitial(savedActionDTO);
            
            savedActionDTO.CrateStorage = initCrateStorageDTO;

            await ExtractData_Execute(savedActionDTO);
        }
        
        /// <summary>
        /// Create ActionController instance.
        /// </summary>
        public ActionController CreateActionController()
        {
            return CreateController<ActionController>(_testUserAccount.Id);
        }

        #region Commented
        //private CrateStorageDTO WaitForDocuSignEvent_ConfigureInitial(ActionDTO curActionDTO)
        //{
        //    // Fill values as it would be on front-end.
        //    curActionDTO.ActivityTemplateId = _waitForDocuSignEventActivityTemplate.Id;
        //    curActionDTO.CrateStorage = new CrateStorageDTO();

        //    // Send initial configure request.
        //    var curActionController = CreateActionController();
        //    var actionDTO = curActionController.Configure(curActionDTO)
        //        as OkNegotiatedContentResult<ActionDTO>;



        //    // Assert initial configuration returned in CrateStorage.
        //    Assert.NotNull(actionDTO);
        //    Assert.NotNull(actionDTO.Content);
        //    Assert.NotNull(actionDTO.Content.CrateStorage.CrateDTO);
        //    Assert.AreEqual(actionDTO.Content.CrateStorage.CrateDTO.Count, 2);
        //    Assert.True((actionDTO.Content.CrateStorage.CrateDTO
        //        .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME)));
        //    Assert.True(actionDTO.Content.CrateStorage.CrateDTO
        //        .Any(x => x.Label == "Available Templates" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));

        //    return actionDTO.Content.CrateStorage;
        //}

        //private void WaitForDocuSignEvent_SelectFirstTemplate(CrateStorageDTO curCrateStorage)
        //{
        //    // Fetch Available Template crate and parse StandardDesignTimeFieldsMS.
        //    var availableTemplatesCrate = curCrateStorage.CrateDTO
        //        .Single(x => x.Label == "Available Templates" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME);

        //    var fieldsMS = JsonConvert.DeserializeObject<StandardDesignTimeFieldsMS>(
        //        availableTemplatesCrate.Contents);

        //    // Fetch Configuration Controls crate and parse StandardConfigurationControlsMS
        //    var configurationControlsCrate = curCrateStorage.CrateDTO
        //        .Single(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);

        //    var controlsMS = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(
        //        configurationControlsCrate.Contents);

        //    // Modify value of Selected_DocuSign_Template field and push it back to crate,
        //    // exact same way we do on front-end.
        //    var docuSignTemplateControl = controlsMS.Controls.Single(x => x.Name == "Selected_DocuSign_Template");
        //    docuSignTemplateControl.Value = fieldsMS.Fields.First().Value;

        //    configurationControlsCrate.Contents = JsonConvert.SerializeObject(controlsMS);
        //}

        //private CrateStorageDTO WaitForDocuSignEvent_ConfigureFollowUp(ActionDTO curActionDTO)
        //{
        //    var curActionController = CreateActionController();

        //    var actionDTO = curActionController.Configure(curActionDTO)
        //        as OkNegotiatedContentResult<ActionDTO>;

        //    // Assert FollowUp Configure result.
        //    Assert.NotNull(actionDTO);
        //    Assert.NotNull(actionDTO.Content);
        //    Assert.NotNull(actionDTO.Content.CrateStorage.CrateDTO);
        //    Assert.AreEqual(3, actionDTO.Content.CrateStorage.CrateDTO.Count);//replace this with 3 when 1123 is fixed
        //    Assert.True(actionDTO.Content.CrateStorage.CrateDTO
        //        .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
        //    //Assert.True(result.Content.CrateDTO   //uncomment this when 1123 is fixed
        //    //  .Any(x => x.Label == "Available Templates" && x.ManifestType == "Standard Design-Time Fields"));
        //    Assert.True(actionDTO.Content.CrateStorage.CrateDTO
        //        .Any(x => x.Label == "DocuSignTemplateUserDefinedFields" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));

        //    return actionDTO.Content.CrateStorage;
        //}

        //private CrateStorageDTO FilterUsingRunTimeData_ConfigureInitial(ActionDTO curActionDTO)
        //{
        //    // Fill values as it would be on front-end.
        //    curActionDTO.ActivityTemplateId = _filterUsingRunTimeDataActivityTemplate.Id;
        //    curActionDTO.CrateStorage = new CrateStorageDTO();

        //    // Send initial configure request.
        //    var curActionController = CreateActionController();
        //    var result = curActionController.Configure(curActionDTO)
        //        as OkNegotiatedContentResult<ActionDTO>;

        //    Assert.NotNull(result);
        //    Assert.NotNull(result.Content);
        //    Assert.NotNull(result.Content.CrateStorage.CrateDTO);
        //    Assert.AreEqual(result.Content.CrateStorage.CrateDTO.Count, 2);
        //    Assert.True(result.Content.CrateStorage.CrateDTO
        //        .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
        //    Assert.True(result.Content.CrateStorage.CrateDTO
        //        .Any(x => x.Label == "Queryable Criteria" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));

        //    return result.Content.CrateStorage;
        //}

        //private CrateStorageDTO WriteToSqlServer_ConfigureInitial(ActionDTO curActionDTO)
        //{
        //    curActionDTO.ActivityTemplateId = _writeToSqlServerActivityTemplate.Id;
        //    curActionDTO.CrateStorage = new CrateStorageDTO();
        //    var curActionController = CreateActionController();
        //    var result = curActionController.Configure(curActionDTO)
        //        as OkNegotiatedContentResult<ActionDTO>;

        //    Assert.NotNull(result);
        //    Assert.NotNull(result.Content);
        //    Assert.NotNull(result.Content.CrateStorage.CrateDTO);
        //    Assert.AreEqual(result.Content.CrateStorage.CrateDTO.Count, 1);
        //    Assert.True(result.Content.CrateStorage.CrateDTO
        //        .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));

        //    return result.Content.CrateStorage;
        //}

        //private void WriteToSqlServer_InputConnectionString(CrateStorageDTO curCrateStorage)
        //{
        //    // Fetch Configuration Controls crate and parse StandardConfigurationControlsMS
        //    var configurationControlsCrate = curCrateStorage.CrateDTO
        //        .Single(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);

        //    var controlsMS = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(
        //        configurationControlsCrate.Contents);

        //    // Modify value of Selected_DocuSign_Template field and push it back to crate,
        //    // exact same way we do on front-end.
        //    var connectionStringControl = controlsMS.Controls.Single(x => x.Name == "connection_string");
        //    connectionStringControl.Value = "Server = tcp:s79ifqsqga.database.windows.net,1433; Database = demodb_health; User ID = alexeddodb@s79ifqsqga; Password = Thales89; Trusted_Connection = False; Encrypt = True; Connection Timeout = 30;";

        //    configurationControlsCrate.Contents = JsonConvert.SerializeObject(controlsMS);
        //}

        //private CrateStorageDTO WriteToSqlServer_ConfigureFollowUp(ActionDTO curActionDTO)
        //{
        //    var curActionController = CreateActionController();

        //    var actionDTO = curActionController.Configure(curActionDTO)
        //        as OkNegotiatedContentResult<ActionDTO>;

        //    // Assert FollowUp Configure result.
        //    Assert.NotNull(actionDTO);
        //    Assert.NotNull(actionDTO.Content);
        //    Assert.NotNull(actionDTO.Content.CrateStorage.CrateDTO);
        //    Assert.AreEqual(2, actionDTO.Content.CrateStorage.CrateDTO.Count);//replace this with 3 when 1123 is fixed
        //    Assert.True(actionDTO.Content.CrateStorage.CrateDTO
        //        .Any(x => x.Label == "Configuration_Controls" && x.ManifestType == CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME));
        //    Assert.True(actionDTO.Content.CrateStorage.CrateDTO
        //        .Any(x => x.Label == "Sql Table Columns" && x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME));

        //    return actionDTO.Content.CrateStorage;
        //}

        ///// <summary>
        ///// Test WaitForDocuSignEvent initial configuration.
        ///// </summary>
        ////[Test]
        //public void PluginIntegration_WaitForDocuSign_ConfigureInitial()
        //{
        //    var savedActionDTO = CreateEmptyAction();
        //    WaitForDocuSignEvent_ConfigureInitial(savedActionDTO);
        //}

        ///// <summary>
        ///// Test WaitForDocuSignEvent follow-up configuration.
        ///// </summary>
        ////[Test]
        //public void PluginIntegration_WaitForDocuSign_ConfigureFollowUp()
        //{
        //    // Create blank WaitForDocuSignEventAction.
        //    var savedActionDTO = CreateEmptyAction();

        //    // Call Configure Initial for ExtractData action.
        //    var initCrateStorageDTO = WaitForDocuSignEvent_ConfigureInitial(savedActionDTO);

        //    // Select first available DocuSign template.
        //    WaitForDocuSignEvent_SelectFirstTemplate(initCrateStorageDTO);
        //    savedActionDTO.CrateStorage = initCrateStorageDTO;

        //    // Call Configure FollowUp for ExtractData action.
        //    WaitForDocuSignEvent_ConfigureFollowUp(savedActionDTO);
        //}

        ///// <summary>
        ///// Test FilterUsingRunTimeData initial configuration.
        ///// </summary>
        ////[Test]
        //public void PluginIntegration_FilterUsingRunTimeData_ConfigureInitial()
        //{
        //    // Create blank ExtractData action.
        //    var waitForDocuSignEventAction = CreateEmptyAction();

        //    // Call Configure Initial for ExtractData action.
        //    var initWaitForDocuSignEventCS = WaitForDocuSignEvent_ConfigureInitial(waitForDocuSignEventAction);

        //    // Select first available DocuSign template.
        //    WaitForDocuSignEvent_SelectFirstTemplate(initWaitForDocuSignEventCS);
        //    waitForDocuSignEventAction.CrateStorage = initWaitForDocuSignEventCS;

        //    // Call Configure FollowUp for ExtractData action.
        //    WaitForDocuSignEvent_ConfigureFollowUp(waitForDocuSignEventAction);

        //    // Save ExtractData action.
        //    SaveAction(waitForDocuSignEventAction);

        //    // Create blank FilterUsingRunTimeData action.
        //    var filterAction = CreateEmptyAction();

        //    // Call Configure Initial for FilterUsingRunTimeData action.
        //    FilterUsingRunTimeData_ConfigureInitial(filterAction);
        //}

        ///// <summary>
        ///// Test WriteToSqlServer initial configuration.
        ///// </summary>
        ////[Test]
        //public void PluginIntegration_WriteToSqlServer_ConfigureInitial()
        //{
        //    // Create blank ExtractData action.
        //    var emptyAction = CreateEmptyAction();

        //    // Call Configure Initial for WriteToSqlServer action.
        //    WriteToSqlServer_ConfigureInitial(emptyAction);
        //}

        ///// <summary>
        ///// Test WriteToSqlServer follow-up configuration.
        ///// </summary>
        ////[Test]
        //public void PluginIntegration_WriteToSqlServer_ConfigureFollowUp()
        //{
        //    // Create blank WaitForDocuSignEventAction.
        //    var savedActionDTO = CreateEmptyAction();

        //    // Call Configure Initial for ExtractData action.
        //    var initCrateStorageDTO = WriteToSqlServer_ConfigureInitial(savedActionDTO);

        //    // Select first available DocuSign template.
        //    WriteToSqlServer_InputConnectionString(initCrateStorageDTO);
        //    savedActionDTO.CrateStorage = initCrateStorageDTO;

        //    // Call Configure FollowUp for ExtractData action.
        //    WriteToSqlServer_ConfigureFollowUp(savedActionDTO);
        //}
        #endregion
    }
}
