using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Infrastructure;
using Microsoft.Ajax.Utilities;
using terminalAsana.Asana;
using terminalAsana.Asana.Entities;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;

namespace terminalAsana.Activities
{
    public class Get_Tasks_v1 : AsanaOAuthBaseActivity<Get_Tasks_v1.ActivityUi>
    {
        public static readonly  ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Get_Tasks",
            Label = "Get Tasks",
            Category = ActivityCategory.Receivers,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            Categories = new[] {
                ActivityCategories.Receive,
                new ActivityCategoryDTO(TerminalData.WebServiceDTO.Name, TerminalData.WebServiceDTO.IconPath)
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        

        private const string RunTimeCrateLabel = "Asana Tasks";
        private const string RunTimeCrateLabelCustomCM = "Asana Tasks List";


        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList WorkspacesList;
            public DropDownList UsersList;
            public DropDownList ProjectsList;
            public TextBlock Information;

            public ActivityUi()
            {
                WorkspacesList = new DropDownList()
                {
                    Label = "Avaliable workspaces",
                    Name = nameof(WorkspacesList),
                    ListItems = new List<ListItem>(),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                };


                ProjectsList = new DropDownList()
                {
                    Label = "Projects in workspace",
                    Name = nameof(ProjectsList),
                    ListItems = new List<ListItem>(),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                Information = new TextBlock()
                {
                    Name = nameof(Information),
                    Label = "If you specify a project, username and workspace won`t be taken in account."
                };

                UsersList = new DropDownList()
                {
                    Label = "Users in workspace",
                    Name = nameof(UsersList),
                    ListItems = new List<ListItem>(),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                Controls = new List<ControlDefinitionDTO>(){ WorkspacesList, UsersList, Information, ProjectsList };
            }


        }

        public Get_Tasks_v1(ICrateManager crateManager, IRestfulServiceClient client, IAsanaParameters parameters) : base(crateManager, parameters, client)
        {
            DisableValidationOnFollowup = true;
        }


        public override async Task Initialize()
        {
            var workspaces = await AClient.Workspaces.GetAsync();
            ActivityUI.WorkspacesList.ListItems = workspaces.Select( w => new ListItem() { Key= w.Name, Value = w.Id} ).ToList();

            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel).AddFields("Task name", "Task id");
            CrateSignaller.MarkAvailableAtRuntime<AsanaTaskListCM>(RunTimeCrateLabelCustomCM);
        }

        public override async Task FollowUp()
        {           
            if (!ActivityUI.WorkspacesList.Value.IsNullOrWhiteSpace())
            {
                var users =  await AClient.Users.GetUsersAsync(ActivityUI.WorkspacesList.Value);
                ActivityUI.UsersList.ListItems = users.Select(w => new ListItem() {Key = w.Name, Value = w.Id}).ToList();                

                var projects = 
                    await AClient.Projects.Get(new AsanaProjectQuery() {Workspace = ActivityUI.WorkspacesList.Value });     
                ActivityUI.ProjectsList.ListItems = projects.Select(w => new ListItem() { Key = w.Name, Value = w.Id }).ToList();                                         
            }
        }

        protected override Task Validate()
        {
            ValidationManager.ValidateDropDownListNotEmpty(ActivityUI.WorkspacesList, "Workspace should not be empty");
            ValidationManager.ValidateDropDownListNotEmpty(ActivityUI.UsersList, "User should not be empty");
 
            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            var query = new AsanaTaskQuery()
            {
                Workspace = ActivityUI.WorkspacesList.Value,
                Assignee = ActivityUI.UsersList.Value,
                Project = ActivityUI.ProjectsList.Value
            };

            var tasks = await AClient.Tasks.GetAsync(query);
            
            var dataRows = tasks.Select(t => new TableRowDTO()
            { Row = {
                new TableCellDTO() {Cell = new KeyValueDTO("Task name",t.Name)},
                new TableCellDTO() {Cell = new KeyValueDTO("Task id",t.Id)}
            }}).ToList();      

            var payload = new StandardTableDataCM() {Table = dataRows};
            var customPayload = new AsanaTaskListCM() {Tasks = tasks.Select(t => Mapper.Map<AsanaTaskCM>(t)).ToList()};

            Payload.Add(RunTimeCrateLabel, payload);
            Payload.Add(RunTimeCrateLabelCustomCM, customPayload);
        }
    }
}