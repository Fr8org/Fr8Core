using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Activities
{


    //////////// DISABLED
    //
    //"This activity is incomplete. It's based on the premise that we will register for and record all DocuSign events for a user, 
    //and give them search capability. We deemphasized it in part because DocuSign invested a bunch of energy into their native reporting,
    //and it's not obviously an improvement on their base functionality."
    //
    //

    public class Search_DocuSign_History_v1 : BaseDocuSignActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("c64f4378-f259-4006-b4f1-f7e90709829e"),
            Name = "Search_DocuSign_History",
            Label = "Search DocuSign History",
            Version = "1",
            NeedsAuthentication = true,
            MinPaneWidth = 380,
            Tags = Tags.Internal,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        internal class ActivityUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public TextBox SearchText { get; set; }

            [JsonIgnore]
            public DropDownList Folder { get; set; }

            [JsonIgnore]
            public DropDownList Status { get; set; }

            public ActivityUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<p>Search for DocuSign Envelopes where the following are true:</p>" +
                            "<div>Envelope contains text:</div>"
                });

                Controls.Add(SearchText = new TextBox
                {
                    Name = "SearchText",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                });

                Controls.Add(Folder = new DropDownList
                {
                    Label = "Envelope is in folder:",
                    Name = "Folder",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Source = null
                });

                Controls.Add(Status = new DropDownList
                {
                    Label = "Envelope has status:",
                    Name = "Status",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Source = null
                });

                Controls.Add(new RunPlanButton());
            }
        }


        protected override string ActivityUserFriendlyName => "Search DocuSign History";

        public Search_DocuSign_History_v1(ICrateManager crateManager, IDocuSignManager docuSignManager)
            : base(crateManager, docuSignManager)
        {
        }

        public override async Task Run()
        {
            Success();
        }

        public override async Task Initialize()
        {
            var actionUi = new ActivityUi();
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(AuthorizationToken.Token);

            AddControls(actionUi.Controls);
            //commented out by FR-2400
            //_docuSignManager.FillFolderSource(configurationCrate, "Folder", docuSignAuthDTO);
            //_docuSignManager.FillStatusSource(configurationCrate, "Status");

            await ConfigureNestedActivities(actionUi);
        }

        public override async Task FollowUp()
        {
            if (ConfigurationControls == null)
            {
                return;
            }
            var actionUi = new ActivityUi();
            actionUi.ClonePropertiesFrom(ConfigurationControls);
            await ConfigureNestedActivities(actionUi);
        }

        private async Task ConfigureNestedActivities(ActivityUi actionUi)
        {
            var config = new Query_DocuSign_v1.ActivityUi
            {
                Folder = { Value = actionUi.Folder.Value },
                Status = { Value = actionUi.Status.Value },
                SearchText = { Value = actionUi.SearchText.Value }
            };

            var template = (await FindTemplates(x => x.Name == "Query_DocuSign"))
                            .FirstOrDefault();

            if (template == null)
            {
                throw new Exception("Can't find activity template: Query_DocuSign");
            }

            var storage = new CrateStorage(Crate.FromContent("Config", config))
            {
                Crate.FromContent(TerminalActivityBase.ConfigurationControlsLabel, new StandardConfigurationControlsCM(
                new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<p>This activity is managed by the parent activity</p>"
                }))
            };

            var activity = ActivityPayload.ChildrenActivities.OfType<ActivityPayload>().FirstOrDefault();

            if (activity == null)
            {
                activity = new ActivityPayload
                {
                    ActivityTemplate = Mapper.Map<ActivityTemplateSummaryDTO>(template),
                    Name = template.Label,
                    Ordering = 1,
                };

                ActivityPayload.ChildrenActivities.Add(activity);
            }
            activity.CrateStorage = storage;

        }

        private async Task<IEnumerable<ActivityTemplateDTO>> FindTemplates(Predicate<ActivityTemplateDTO> query)
        {
            var templates = await HubCommunicator.GetActivityTemplates(true);
            return templates.Where(x => query(x));
        }
    }
}