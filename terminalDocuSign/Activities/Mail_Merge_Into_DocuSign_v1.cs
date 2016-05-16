using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using TerminalBase.Infrastructure;
using TerminalBase.Infrastructure.Behaviors;

namespace terminalDocuSign.Actions
{
    public class Mail_Merge_Into_DocuSign_v1 : BaseDocuSignActivity
    {

        private string _dataSourceValue;

        private DropDownList _docuSignTemplate;

        private const string SolutionName = "Mail Merge Into DocuSign";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "DocuSign";

        private const string SolutionBody = @"<p>This solution is designed to take data from any table-like source(initially supported: Microsoft Excel and Google Sheets) and create and send DocuSign Envelopes.A DocuSign Template is used to generate the envelopes, and Fr8 makes it easy to map data from the sources to the DocuSign Template for automatic insertion.</p>
                                              <p>This Activity also highlights the use of the Loop activity, which can process any amount of table data, one row at a time.</p>
                                              <iframe src='https://player.vimeo.com/video/162762690' width='500' height='343' frameborder='0' webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>";

       
        protected override string ActivityUserFriendlyName => SolutionName;

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
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
                ListItems = await GetDataSourceListItems("Table Data Generator"),
                Required = true
            });

            controlList.Add(CreateDocuSignTemplatePicker(false, "DocuSignTemplate", "2. Use which DocuSign Template?"));
            controlList.Add(new Button()
            {
                Label = "Prepare Mail Merge",
                Name = "Continue",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onClick", "requestConfig")
                }
            });

            return PackControlsCrate(controlList.ToArray());
        }

        private async Task<List<ListItem>> GetDataSourceListItems(string tag)
        {
            var curActivityTemplates = await HubCommunicator.GetActivityTemplates(tag, CurrentFr8UserId);
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
                        //build a controls crate to render the pane
                        var configurationCrate = await CreateConfigurationControlsCrate(curActivityDO);
                        FillDocuSignTemplateSource(configurationCrate, "DocuSignTemplate", authTokenDO);
                        crateStorage.Add(configurationCrate);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Configuration requires the submission of an Action that has a real ActionId");
            }

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

        private T GetStdConfigurationControl<T>(ICrateStorage storage, string name) where T : ControlDefinitionDTO
        {
            var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            if (controls == null)
            {
                return null;
            }
            var control = (T)controls.FindByName(name);
            return control;
        }

        public override Task ValidateActivity(ActivityDO curActivityDO, ICrateStorage crateStorage, ValidationManager validationManager)
        {
            var templateList = GetStdConfigurationControl<DropDownList>(crateStorage, "DocuSignTemplate");

            if (!validationManager.ValidateControlExistance(templateList))
            {
                return Task.FromResult(0);
            }

            validationManager.ValidateTemplateList(templateList);
            
            var sourceConfigControl = GetStdConfigurationControl<DropDownList>(crateStorage, "DataSource");

            if (!validationManager.ValidateControlExistance(sourceConfigControl))
            {
                return Task.FromResult(0);
            }

            if (DocuSignValidationUtils.AtLeastOneItemExists(sourceConfigControl))
            {
                if (!DocuSignValidationUtils.ItemIsSelected(sourceConfigControl))
                {
                    validationManager.SetError("Data source is not selected", sourceConfigControl);
                }
            }
            else
            {
                validationManager.SetError("No data source exists", sourceConfigControl);
            }

            return Task.FromResult(0);
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
            if (dataSource.Value != null)
                _dataSourceValue = dataSource.Value;

            _docuSignTemplate = GetStdConfigurationControl<DropDownList>(storage, "DocuSignTemplate");

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

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var reconfigList = new List<ConfigurationRequest>()
            {
                new ConfigurationRequest()
                {
                    HasActivityMethod = HasFirstChildActivity,
                    CreateActivityMethod = CreateFirstChildActivity,
                    ConfigureActivityMethod = ConfigureFirstChildActivity,
                    ChildActivityIndex = 1
                },
                new ConfigurationRequest()
                {
                    HasActivityMethod = HasSecondChildActivity,
                    CreateActivityMethod = CreateSecondChildActivity,
                    ConfigureActivityMethod = ConfigureSecondChildActivity,
                    ChildActivityIndex = 2
                }
            };

            var behavior = new ReconfigurationListBehavior(this);
            await behavior.ReconfigureActivities(curActivityDO, authTokenDO, reconfigList);
            return await Task.FromResult(curActivityDO);
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

        /// <summary>
        /// TODO this part is ugly - why do we load those activities from hub in activity
        /// we already have a code in base class that does this operation with caching
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<ActivityDO> CreateFirstChildActivity(ReconfigurationContext context)
        {
            var curActivityTemplates = (await HubCommunicator.GetActivityTemplates(null))
                .Select(Mapper.Map<ActivityTemplateDO>)
                .ToList();

            // Let's check if activity template generates table data
            var selectedReceiver = curActivityTemplates.Single(x => x.Name == _dataSourceValue);

            var selectedReceiverTemplate = await GetActivityTemplate(selectedReceiver.Id);

            var dataSourceActivity = await AddAndConfigureChildActivity(
                context.SolutionActivity,
                selectedReceiverTemplate,
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
                .Select(Mapper.Map<ActivityTemplateDO>)
                .ToList();

            var selectedReceiver = curActivityTemplates.Single(x => x.Name == _dataSourceValue);
            ActivityDO parentActivity;
            int activityIndex;

            if (DoesActivityTemplateGenerateTableData(selectedReceiver))
            {
                var loopAT = await GetActivityTemplate("terminalFr8Core", "Loop");
                var loopActivity = await AddAndConfigureChildActivity(context.SolutionActivity, loopAT, "Loop", "Loop", 2);

                using (var crateStorage = CrateManager.GetUpdatableStorage(loopActivity))
                {
                    var loopConfigControls = GetConfigurationControls(crateStorage);
                    var crateChooser = GetControl<CrateChooser>(loopConfigControls, "Available_Crates");
                    var tableDescription = crateChooser.CrateDescriptions.FirstOrDefault(c => c.ManifestId == (int)MT.StandardTableData);
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

            var sendDocusignEnvelopeAT = await GetActivityTemplate("terminalDocuSign", "Send_DocuSign_Envelope");
            var sendDocuSignActivity = await AddAndConfigureChildActivity(parentActivity, sendDocusignEnvelopeAT, order: activityIndex);
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
                    throw new ApplicationException("Invalid solution structure, no Loop activity found.");
                }

                loopActivity = await ConfigureChildActivity(context.SolutionActivity, loopActivity);

                using (var crateStorage = CrateManager.GetUpdatableStorage(loopActivity))
                {
                    var loopConfigControls = GetConfigurationControls(crateStorage);
                    var crateChooser = GetControl<CrateChooser>(loopConfigControls, "Available_Crates");
                    var tableDescription = crateChooser.CrateDescriptions.FirstOrDefault(c => c.ManifestId == (int)MT.StandardTableData);
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

            var sendDocuSignEnvelope = parentActivity.ChildNodes
                .OfType<ActivityDO>()
                .Single(x => x.ActivityTemplate.Name == "Send_DocuSign_Envelope"
                    && x.Ordering == activityIndex
                );

            sendDocuSignEnvelope.CrateStorage = string.Empty;
            sendDocuSignEnvelope = await ConfigureChildActivity(parentActivity, sendDocuSignEnvelope);

            SetControlValue(
                sendDocuSignEnvelope,
                "target_docusign_template",
                _docuSignTemplate.ListItems
                    .FirstOrDefault(a => a.Key == _docuSignTemplate.selectedKey)
            );

            sendDocuSignEnvelope = await ConfigureChildActivity(parentActivity, sendDocuSignEnvelope);

            return sendDocuSignEnvelope;
        }

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
                    return Task.FromResult(GenerateDocumentationResponse(@"This solution helps you to work with email and move data from them to DocuSign service"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(GenerateDocumentationResponse(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(GenerateErrorResponse("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    GenerateErrorResponse("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }
    }
}