using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Infrastructure;
using Microsoft.Ajax.Utilities;
using terminalAsana.Asana;
using terminalAsana.Asana.Entities;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;

namespace terminalAsana.Activities
{
    public class Post_Comment_v1 : AsanaOAuthBaseActivity<Post_Comment_v1.ActivityUi>
    {
        public static readonly ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("0ee8bf8f-941e-4861-beb8-d7d98536eba8"),
            Name = "Post_Comment",
            Label = "Post Comment",
            Version = "1",
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            Categories = new[] {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        


        private const string RunTimeCrateLabel = "Post Comment";
        private const string ResultFieldLabel = "ActivityResult";


        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList Workspaces { get; set; }
            public DropDownList Projects { get; set; }
            public DropDownList Tasks { get; set; }
            public TextSource   Comment { get; set; }

            public ActivityUi()
            {
                Workspaces = new DropDownList
                {
                    Label = "Select Workspace",
                    Name = nameof(Workspaces),
                    Required = true,
                    ListItems = new List<ListItem>(),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Projects = new DropDownList
                {
                    Label = "Select Project",
                    Name = nameof(Projects),
                    Required = true,
                    ListItems = new List<ListItem>(),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                Tasks = new DropDownList
                {
                    Label = "Select Task",
                    Name = nameof(Tasks),
                    Required = true,
                    ListItems = new List<ListItem>(),
                };
                Comment = new TextSource
                {
                    InitialLabel = "Comment",
                    Label = "Comment",
                    Name = nameof(Comment),
                    GroupLabelText = "",
                    
                    Source = new FieldSourceDTO
                    {
                        RequestUpstream = true
                    }
                };
                Controls = new List<ControlDefinitionDTO> { Workspaces, Projects, Tasks, Comment };
            }
        }

        public Post_Comment_v1(ICrateManager crateManager,  IAsanaParameters parameters, IRestfulServiceClient client)
            : base(crateManager, parameters, client)
        {
            DisableValidationOnFollowup = true;
        }

        protected override void InitializeInternalState()
        {
            base.InitializeInternalState();
        }

        public override async Task Initialize()
        {
            try
            {
                var workspaces = await AClient.Workspaces.GetAsync();
                ActivityUI.Workspaces.ListItems = workspaces.Select(w => new ListItem() { Key = w.Name, Value = w.Id }).ToList();
            }
            catch (Exception exp)
            {
                Logger.GetLogger("terminalAsana").Error("Error ocured while initializing Post_Comment_v1 Asana activity", exp);
                throw exp;
            }
            
        }

        public override async Task FollowUp()
        {
            try
            {
                if (!ActivityUI.Workspaces.Value.IsNullOrWhiteSpace())
                {
                    var projects = await AClient.Projects.Get(new AsanaProjectQuery() { Workspace = ActivityUI.Workspaces.Value });
                    ActivityUI.Projects.ListItems = projects.Select(w => new ListItem() { Key = w.Name, Value = w.Id }).ToList();

                    IEnumerable<AsanaTask> tasks;
                    if (!ActivityUI.Projects.Value.IsNullOrWhiteSpace())
                    {
                        tasks = await AClient.Tasks.GetAsync(new AsanaTaskQuery() { Project = ActivityUI.Projects.Value });
                    }
                    else
                    {
                        tasks = await AClient.Tasks.GetAsync(new AsanaTaskQuery() { Workspace = ActivityUI.Workspaces.Value });
                    }

                    ActivityUI.Tasks.ListItems = tasks.Select(w => new ListItem() { Key = w.Name, Value = w.Id }).ToList();
                }
            }
            catch (Exception exp)
            {
                Logger.GetLogger("terminalAsana").Error("Error ocured while followup configuring Post_Comment_v1 Asana activity", exp);
                throw exp;
            }
            
            
        }

        protected override Task Validate()
        {

            ValidationManager.ValidateDropDownListNotEmpty(ActivityUI.Workspaces, "No workspace was selected");
            ValidationManager.ValidateDropDownListNotEmpty(ActivityUI.Tasks, "No task was selected");
            ValidationManager.ValidateTextSourceNotEmpty(ActivityUI.Comment, "No data was entered for Comment");

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var taskId = ActivityUI.Tasks.Value;
            var payloadMessage = ActivityUI.Comment.TextValue;

            var comment = await AClient.Stories.PostCommentAsync(taskId, payloadMessage);            
        }
    }
}