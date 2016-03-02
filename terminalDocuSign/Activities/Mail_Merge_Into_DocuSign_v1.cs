using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.Manifests;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using Utilities.Configuration.Azure;
using terminalDocuSign.Infrastructure;
using Data.Constants;
using Data.Repositories;
using Data.States;
using UtilitiesTesting.Fixtures;

namespace terminalDocuSign.Actions
{
    public class Mail_Merge_Into_DocuSign_v1 : BaseDocuSignActivity
    {
        readonly DocuSignManager _docuSignManager;
        string _dataSourceValue;
        DropDownList _docuSignTemplate;


        string _docuSignTemplateValue;
        private const string SolutionName = "Mail Merge Into DocuSign";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "DocuSign";
        private const string SolutionBody = @"<p>Pull data from a variety of sources, including Excel files, 
                                            Google Sheets, and databases, and merge the data into your DocuSign template. 
                                            You can link specific fields from your source data to DocuSign fields</p>";


        public Mail_Merge_Into_DocuSign_v1()
            : base()
        {
            _docuSignManager = new DocuSignManager();
        }

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            /*
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            var storage = Crate.GetStorage(curActivityDO);
            DropDownList docuSignTemplate = GetStdConfigurationControl<DropDownList>(storage, "DocuSignTemplate");
            string envelopeId = docuSignTemplate.Value;

            // Make sure that it exists
            if (string.IsNullOrEmpty(envelopeId))
            {
                return Error(payloadCrates, "EnvelopeId", ActionErrorCode.PAYLOAD_DATA_MISSING);
            }

            //Create run-time fields
            var fields = CreateDocuSignEventFields();
            foreach (var field in fields)
            {
                field.Value = GetValueForKey(payloadCrates, field.Key);
            }

            using (var crateStorage = Crate.GetUpdatableStorage(payloadCrates))
            {
                updater.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Payload Data", new StandardPayloadDataCM(fields)));

                //var userDefinedFieldsPayload = _docuSignManager.CreateActionPayload(curActivityDO, authTokenDO, envelopeId);
                //updater.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Data", userDefinedFieldsPayload));
            }
            */
            //i (bahadir) think solutions should not do anything on their run method
            //they are just preconfiguring existing activities
            return Success(await GetPayload(curActivityDO, containerId));
        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private async Task<Crate> CreateConfigurationControlsCrate(ActivityDO activityDO)
        {
            var controlList = new List<ControlDefinitionDTO>();

            controlList.Add(new DropDownList()
            {
                Label = "1. Where is your Source Data?",
                Name = "DataSource",
                ListItems = await GetDataSourceListItems(activityDO, "Table Data Generator")
            });

            controlList.Add(DocuSignManager.CreateDocuSignTemplatePicker(false, "DocuSignTemplate", "2. Use which DocuSign Template?"));
            controlList.Add(new Button()
            {
                Label = "Continue",
                Name = "Continue",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onClick", "requestConfig")
                }
            });

            return PackControlsCrate(controlList.ToArray());
        }

        private async Task<List<ListItem>> GetDataSourceListItems(ActivityDO activityDO, string tag)
        {
            var curActivityTemplates = await HubCommunicator.GetActivityTemplates(tag)
                .ContinueWith(x => x.Result.Where(y => y.Name.StartsWith("Get", StringComparison.InvariantCultureIgnoreCase) && y.Category == Data.States.ActivityCategory.Receivers));
            return curActivityTemplates.Select(at => new ListItem() { Key = at.Label, Value = at.Name }).ToList();
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActivityDO.Id != Guid.Empty)
            {
                using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                {
                    if (authTokenDO == null || authTokenDO.Token == null)
                    {
                        crateStorage.Replace(new CrateStorage(await CreateNoAuthCrate()));
                    }
                    else
                    {
                        var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

                        //build a controls crate to render the pane
                        var configurationControlsCrate = await CreateConfigurationControlsCrate(curActivityDO);
                        var templatesFieldCrate = _docuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO);

                        crateStorage.Add(configurationControlsCrate);
                        crateStorage.Add(templatesFieldCrate);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Configuration requires the submission of an Action that has a real ActionId");
            }

            //validate if any DocuSignTemplates has been linked to the Account
            ValidateDocuSignAtLeastOneTemplate(curActivityDO);
            return curActivityDO;
        }

        private Task<Crate> CreateNoAuthCrate()
        {
            var controlList = new List<ControlDefinitionDTO>();

            controlList.Add(new TextBlock()
            {
                Value = "This activity requires authentication. Please authenticate."
            });
            return Task.FromResult((Crate)PackControlsCrate(controlList.ToArray()));
        }

        private T GetStdConfigurationControl<T>(ICrateStorage storage, string name)
            where T : ControlDefinitionDTO
        {
            var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            if (controls == null)
            {
                return null;
            }

            var control = (T)controls.FindByName(name);
            return control;
        }


        /// <summary>
        /// All validation scenarios for Mail_Merge_Into_DocuSign action
        /// </summary>
        /// <param name="curActivityDO"></param>
        /// <returns></returns>
        protected override async Task<ICrateStorage> ValidateActivity(ActivityDO curActivityDO)
        {
            ValidateDocuSignAtLeastOneTemplate(curActivityDO);

            return await Task.FromResult<ICrateStorage>(null);
        }

        private void ValidateDocuSignAtLeastOneTemplate(ActivityDO curActivityDO)
        {
            //validate DocuSignTemplate for present selected template 
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var docuSignTemplate = crateStorage.CrateContentsOfType<FieldDescriptionsCM>(x => x.Label == "Available Templates").FirstOrDefault();
                if (docuSignTemplate != null && docuSignTemplate.Fields != null && docuSignTemplate.Fields.Count != 0) return;//await Task.FromResult<CrateDTO>(null);

                var configControl = GetStdConfigurationControl<DropDownList>(crateStorage, "DocuSignTemplate");
                if (configControl != null)
                {
                    configControl.ErrorMessage = "Please link some templates to your DocuSign account.";
                }
            }
        }

        /// <summary>
        /// If there's a value in select_file field of the crate, then it is a followup call.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            // Do not tarsnfer to follow up when child actions are already present 
            //if (curActivityDO.ChildNodes.Any()) return ConfigurationRequestType.Initial;

            var storage = CrateManager.GetStorage(curActivityDO);
            if (storage == null || !storage.Any())
            {
                return ConfigurationRequestType.Initial;
            }

            // "Follow up" phase is when Continue button is clicked 
            Button button = GetStdConfigurationControl<Button>(storage, "Continue");
            if (button == null) return ConfigurationRequestType.Initial;
            if (button.Clicked == false &&
                (curActivityDO.ChildNodes == null || curActivityDO.ChildNodes.Count == 0))
            {
                return ConfigurationRequestType.Initial;
            }

            // If no values selected in textboxes, remain on initial phase
            DropDownList dataSource = GetStdConfigurationControl<DropDownList>(storage, "DataSource");
            if (dataSource.Value == null) return ConfigurationRequestType.Initial;
            _dataSourceValue = dataSource.Value;

            _docuSignTemplate = GetStdConfigurationControl<DropDownList>(storage, "DocuSignTemplate");
            if (_docuSignTemplate.Value == null) return ConfigurationRequestType.Initial;

            return ConfigurationRequestType.Followup;
        }

        /// <summary>
        /// Checks if activity template generates table data
        /// TODO: find a smoother (unified) way for this
        /// </summary>
        /// <returns></returns>
        private bool DoesActivityTemplateGenerateTableData(ActivityTemplateDO activityTemplate)
        {
            return activityTemplate.Tags != null && activityTemplate.Tags.Split(',').Any(t => t.ToLowerInvariant().Contains("table"));
        }

        // TODO: FR-2488, to be moved to separate behavior class.
        class ReconfigurationItem
        {
            public Func<ReconfigurationContext, Task<bool>> HasActivityMethod { get; set; }
            public Func<ReconfigurationContext, Task<ActivityDO>> CreateActivityMethod { get; set; }
            public Func<ReconfigurationContext, Task<ActivityDO>> ConfigureActivityMethod { get; set; }
            public int ChildActivityIndex { get; set; }
        }

        class ReconfigurationContext
        {
            public ReconfigurationContext()
            {
                AdditionalItems = new List<ReconfigurationItem>();
            }

            public ActivityDO SolutionActivity { get; set; }
            public AuthorizationTokenDO AuthToken { get; set; }
            public IReadOnlyList<ReconfigurationItem> Items { get; set; }
            public List<ReconfigurationItem> AdditionalItems { get; set; }
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(
            ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                // extract fields in docusign form
                _docuSignManager.UpdateUserDefinedFields(
                    curActivityDO,
                    authTokenDO,
                    updater,
                    _docuSignTemplate.Value
                );
            }

            var reconfigList = new List<ReconfigurationItem>()
            {
                new ReconfigurationItem()
                {
                    HasActivityMethod = HasFirstChildActivity,
                    CreateActivityMethod = CreateFirstChildActivity,
                    ConfigureActivityMethod = ConfigureFirstChildActivity,
                    ChildActivityIndex = 1
                },
                new ReconfigurationItem()
                {
                    HasActivityMethod = HasSecondChildActivity,
                    CreateActivityMethod = CreateSecondChildActivity,
                    ConfigureActivityMethod = ConfigureSecondChildActivity,
                    ChildActivityIndex = 2
                }
            };

            await ReconfigureActivities(curActivityDO, authTokenDO, reconfigList);

            return await Task.FromResult(curActivityDO);
        }

        private async Task ReconfigureActivities(ActivityDO solution,
            AuthorizationTokenDO authToken, IReadOnlyList<ReconfigurationItem> items)
        {
            var queue = new Queue<ReconfigurationItem>(items);

            if (solution.ChildNodes == null)
            {
                solution.ChildNodes = new List<RouteNodeDO>();
            }

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();

                var context = new ReconfigurationContext()
                {
                    SolutionActivity = solution,
                    AuthToken = authToken,
                    Items = items
                };

                if (!await item.HasActivityMethod(context))
                {
                    var childActivityByIndex = solution.ChildNodes
                        .SingleOrDefault(x => x.Ordering == item.ChildActivityIndex);

                    if (childActivityByIndex != null)
                    {
                        await HubCommunicator.DeleteActivity(childActivityByIndex.Id, CurrentFr8UserId);
                        solution.ChildNodes.Remove(childActivityByIndex);
                    }

                    await item.CreateActivityMethod(context);
                }
                else
                {
                    await item.ConfigureActivityMethod(context);
                }

                if (context.AdditionalItems.Count > 0)
                {
                    foreach (var additionalItem in context.AdditionalItems)
                    {
                        if (!queue.Any(x => x.ChildActivityIndex == additionalItem.ChildActivityIndex))
                        {
                            queue.Enqueue(additionalItem);
                        }
                    }
                }
            }
        }

        private Task<bool> HasFirstChildActivity(ReconfigurationContext context)
        {
            if (context.SolutionActivity.ChildNodes == null)
            {
                return Task.FromResult(false);
            }

            var activityExists = context.SolutionActivity
                .ChildNodes
                .OfType<ActivityDO>()
                .Any(x => x.ActivityTemplate.Name == _dataSourceValue
                    && x.Ordering == 1
                );

            return Task.FromResult(activityExists);
        }

        private async Task<ActivityDO> CreateFirstChildActivity(ReconfigurationContext context)
        {
            var curActivityTemplates = (await HubCommunicator.GetActivityTemplates(null))
                .Select(x => Mapper.Map<ActivityTemplateDO>(x))
                .ToList();
            
            // Let's check if activity template generates table data
            var selectedReceiver = curActivityTemplates.Single(x => x.Name == _dataSourceValue);
            var dataSourceActivity = await AddAndConfigureChildActivity(
                context.SolutionActivity,
                selectedReceiver.Id.ToString(),
                order: 1
            );

            context.SolutionActivity.ChildNodes.Remove(dataSourceActivity);
            context.SolutionActivity.ChildNodes.Insert(0, dataSourceActivity);

            return dataSourceActivity;
        }

        private async Task<ActivityDO> ConfigureFirstChildActivity(ReconfigurationContext context)
        {
            var activity = context.SolutionActivity.ChildNodes
                .OfType<ActivityDO>()
                .Single(x => x.Ordering == 1);

            activity.CrateStorage = string.Empty;

            activity = await HubCommunicator.ConfigureActivity(activity, CurrentFr8UserId);

            return activity;
        }

        private async Task<bool> HasSecondChildActivity(ReconfigurationContext context)
        {
            if (context.SolutionActivity.ChildNodes == null)
            {
                return false;
            }

            var curActivityTemplates = (await HubCommunicator.GetActivityTemplates(null))
                .Select(x => Mapper.Map<ActivityTemplateDO>(x))
                .ToList();

            var selectedReceiver = curActivityTemplates.Single(x => x.Name == _dataSourceValue);
            ActivityDO parentActivity;
            int activityIndex;

            if (DoesActivityTemplateGenerateTableData(selectedReceiver))
            {
                var loopActivity = context.SolutionActivity.ChildNodes
                    .OfType<ActivityDO>()
                    .SingleOrDefault(x => x.ActivityTemplate.Name == "Loop" && x.Ordering == 2);

                if (loopActivity == null)
                {
                    return false;
                }

                parentActivity = loopActivity;
                activityIndex = 1;
            }
            else
            {
                parentActivity = context.SolutionActivity;
                activityIndex = 2;
            }

            if (parentActivity.ChildNodes.Count != 1)
            {
                return false;
            }

            var sendDocuSignEnvelope = parentActivity.ChildNodes
                .OfType<ActivityDO>()
                .SingleOrDefault(x => x.ActivityTemplate.Name == "Send_DocuSign_Envelope"
                    && x.Ordering == activityIndex
                );

            return (sendDocuSignEnvelope != null);
        }

        private async Task<ActivityDO> CreateSecondChildActivity(ReconfigurationContext context)
        {
            var curActivityTemplates = (await HubCommunicator.GetActivityTemplates(null))
                .Select(x => Mapper.Map<ActivityTemplateDO>(x))
                .ToList();

            var selectedReceiver = curActivityTemplates.Single(x => x.Name == _dataSourceValue);
            ActivityDO parentActivity;
            int activityIndex;

            if (DoesActivityTemplateGenerateTableData(selectedReceiver))
            {
                var loopActivity = await AddAndConfigureChildActivity(
                    context.SolutionActivity, "Loop", "Loop", "Loop", 2);

                using (var crateStorage = CrateManager.GetUpdatableStorage(loopActivity))
                {
                    var loopConfigControls = GetConfigurationControls(crateStorage);
                    var crateChooser = GetControl<CrateChooser>(loopConfigControls, "Available_Crates");
                    var tableDescription = crateChooser.CrateDescriptions.FirstOrDefault(c => c.ManifestId == (int) MT.StandardTableData);
                    if (tableDescription != null)
                    {
                        tableDescription.Selected = true;
                    }
                }

                parentActivity = loopActivity;
                activityIndex = 1;
            }
            else
            {
                parentActivity = context.SolutionActivity;
                activityIndex = 2;
            }

            var sendDocuSignActivity = await AddAndConfigureChildActivity(parentActivity, "Send_DocuSign_Envelope", order: activityIndex);
            // Set docusign template
            SetControlValue(
                sendDocuSignActivity,
                "target_docusign_template",
                _docuSignTemplate.ListItems
                    .FirstOrDefault(a => a.Key == _docuSignTemplate.selectedKey)
            );
            
            await ConfigureChildActivity(parentActivity, sendDocuSignActivity);

            return activityIndex == 1 ? sendDocuSignActivity : parentActivity;
        }

        private async Task<ActivityDO> ConfigureSecondChildActivity(ReconfigurationContext context)
        {
            var curActivityTemplates = (await HubCommunicator.GetActivityTemplates(null))
                .Select(x => Mapper.Map<ActivityTemplateDO>(x))
                .ToList();

            var selectedReceiver = curActivityTemplates.Single(x => x.Name == _dataSourceValue);
            ActivityDO parentActivity;
            int activityIndex;

            if (DoesActivityTemplateGenerateTableData(selectedReceiver))
            {
                var loopActivity = context.SolutionActivity.ChildNodes
                    .OfType<ActivityDO>()
                    .SingleOrDefault(x => x.ActivityTemplate.Name == "Loop" && x.Ordering == 2);

                if (loopActivity == null)
                {
                    throw new ApplicationException("Invalid solution structure.");
                }

                parentActivity = loopActivity;
                activityIndex = 1;
            }
            else
            {
                parentActivity = context.SolutionActivity;
                activityIndex = 2;
            }

            var sendDocuSignEnvelope = parentActivity.ChildNodes
                .OfType<ActivityDO>()
                .Single(x => x.ActivityTemplate.Name == "Send_DocuSign_Envelope"
                    && x.Ordering == activityIndex
                );

            sendDocuSignEnvelope.CrateStorage = string.Empty;

            sendDocuSignEnvelope = await HubCommunicator.ConfigureActivity(sendDocuSignEnvelope, CurrentFr8UserId);

            SetControlValue(
                sendDocuSignEnvelope,
                "target_docusign_template",
                _docuSignTemplate.ListItems
                    .FirstOrDefault(a => a.Key == _docuSignTemplate.selectedKey)
            );

            sendDocuSignEnvelope = await ConfigureChildActivity(parentActivity, sendDocuSignEnvelope);

            return sendDocuSignEnvelope;
        }

        // TODO: FR-2488, to be removed.
        // if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        // protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        // {
        //     var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
        // 
        //     // TODO: FR-2488, modify code here. 
        //     // We do not want to delete previously created child activities.
        //     if (curActivityDO.ChildNodes.Any())
        //     {
        //         await HubCommunicator.DeleteExistingChildNodesFromActivity(curActivityDO.Id, CurrentFr8UserId);
        // 
        //         curActivityDO.ChildNodes = new List<RouteNodeDO>();
        //     }
        // 
        //     //extract fields in docusign form
        //     _docuSignManager.UpdateUserDefinedFields(curActivityDO, authTokenDO, CrateManager.GetUpdatableStorage(curActivityDO), _docuSignTemplate.Value);
        // 
        //     var curActivityTemplates = (await HubCommunicator.GetActivityTemplates(null))
        //         .Select(x => Mapper.Map<ActivityTemplateDO>(x))
        //         .ToList();
        // 
        //     //let's check if activity template generates table data
        //     var selectedReceiver = curActivityTemplates.Single(x => x.Name == _dataSourceValue);
        //     var dataSourceActivity = await AddAndConfigureChildActivity(
        //         curActivityDO,
        //         selectedReceiver.Id.ToString(),
        //         order: 1
        //     );
        // 
        //     ActivityDO parentOfSendDocusignEnvelope = null;
        //     int orderOfSendDocusignEnvelope = 0;
        // 
        //     // We check whether first activity generates tabled data.
        //     if (DoesActivityTemplateGenerateTableData(selectedReceiver))
        //     {
        //         // If first activity does generate tabled data, we first create Loop,
        //         // and then add SendDocuSignEnvelope inside Loop activity.
        // 
        //         // Let's get first table related CrateDescription in upstream activities and apply it to Loop
        //         var loopActivity = await AddAndConfigureChildActivity(curActivityDO, "Loop", "Loop", "Loop", 2);
        //         using (var crateStorage = CrateManager.GetUpdatableStorage(loopActivity))
        //         {
        //             var loopConfigControls = GetConfigurationControls(crateStorage);
        //             var crateChooser = GetControl<CrateChooser>(loopConfigControls, "Available_Crates");
        //             var tableDescription = crateChooser.CrateDescriptions.FirstOrDefault(c => c.ManifestId == (int) MT.StandardTableData);
        //             if (tableDescription != null)
        //             {
        //                 tableDescription.Selected = true;
        //             }
        //         }
        // 
        //         parentOfSendDocusignEnvelope = loopActivity;
        //         orderOfSendDocusignEnvelope = 1;
        //     }
        //     else
        //     {
        //         // If not, we add SendDocuSignEnvelope activity to solution's activity.
        // 
        //         parentOfSendDocusignEnvelope = curActivityDO;
        //         orderOfSendDocusignEnvelope = 2;
        //     }
        // 
        //     var sendDocuSignEnvActivity = await AddAndConfigureChildActivity(parentOfSendDocusignEnvelope, "Send_DocuSign_Envelope", order: orderOfSendDocusignEnvelope);
        //     //set docusign template
        //     SetControlValue(sendDocuSignEnvActivity, "target_docusign_template", _docuSignTemplate.ListItems.FirstOrDefault(a => a.Key == _docuSignTemplate.selectedKey));
        // 
        // 
        //     await ConfigureChildActivity(parentOfSendDocusignEnvelope, sendDocuSignEnvActivity);
        //     return await Task.FromResult(curActivityDO);
        // }


        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityDO"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        public dynamic Documentation(ActivityDO activityDO, string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = GetDefaultDocumentation(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
              
            }
            if (curDocumentation.Contains("HelpMenu"))
            {
                if (curDocumentation.Contains("ExplainMailMerge"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonce(@"This solution helps you to work with email and move data from them to DocuSign service"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonce(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(GenerateErrorRepsonce("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    GenerateErrorRepsonce("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }
    }
}