using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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
using Microsoft.Ajax.Utilities;
using terminalAsana.Asana;
using terminalAsana.Asana.Entities;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;

namespace terminalAsana.Activities
{
    public class Get_Tasks_v1 : AsanaOAuthBaseActivity<Get_Tasks_v1.ActivityUi>
    {
        private IAsanaWorkspaces _workspaces;
        private IAsanaUsers _users;
        private IAsanaTasks _tasks;
        private IAsanaProjects _projects;


        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Get_Tasks",
            Label = "GetAsync Tasks",
            Category = ActivityCategory.Receivers,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        

        private const string RunTimeCrateLabel = "GetAsync Tasks";
        private const string ResultFieldLabel = "ActivityResult";


        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList WorkspacesList;
            public DropDownList UsersList;
            public DropDownList ProjectsList;

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

                UsersList = new DropDownList()
                {
                    Label = "Users in workspace",
                    Name = nameof(UsersList),
                    ListItems = new List<ListItem>(),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                Controls = new List<ControlDefinitionDTO>(){ WorkspacesList, UsersList, ProjectsList };
            }


        }

        public Get_Tasks_v1(ICrateManager crateManager, IAsanaOAuth oAuth, IRestfulServiceClient client) : base(crateManager, oAuth, client)
        {
            DisableValidationOnFollowup = true;
        }

        protected override void InitializeInternalState()
        {
            base.InitializeInternalState();

            var asanaParams = new AsanaParametersService();
            _workspaces = new Workspaces(OAuthCommunicator, asanaParams);
            _users = new Users(OAuthCommunicator, asanaParams);
            _tasks = new Tasks(OAuthCommunicator, asanaParams);
            _projects = new Projects(OAuthCommunicator, asanaParams);
        }

        public override async Task Initialize()
        {
            var workspaces = _workspaces.Get();
            ActivityUI.WorkspacesList.ListItems = workspaces.Select( w => new ListItem() { Key= w.Name, Value = w.Id} ).ToList();

            CrateSignaller.MarkAvailableAlways<KeyValueListCM>(RunTimeCrateLabel)
                .AddField(new FieldDTO("Task name", AvailabilityType.Always))
                .AddField(new FieldDTO("Task id",AvailabilityType.Always));

        }

        public override async Task FollowUp()
        {
            if (!ActivityUI.WorkspacesList.Value.IsNullOrWhiteSpace())
            {
                var users =  await _users.GetUsersAsync(ActivityUI.WorkspacesList.Value);
                ActivityUI.UsersList.ListItems = users.Select(w => new ListItem() {Key = w.Name, Value = w.Id}).ToList();

                var projects =
                    await _projects.Get(new AsanaProjectQuery() {Workspace = ActivityUI.WorkspacesList.Value });
                ActivityUI.ProjectsList.ListItems = projects.Select(w => new ListItem() { Key = w.Name, Value = w.Id }).ToList();
            }
            
            //Crate
            //Storage - design time
            //Payload - run time
            //CrateManager
            //CrateSignaller
        }

        protected override Task Validate()
        {
            if (ActivityUI.WorkspacesList.Value.IsNullOrWhiteSpace())
            {
                ValidationManager.SetError("Workspace should not be empty", nameof(ActivityUI.WorkspacesList));
            }

            if (ActivityUI.UsersList.Value.IsNullOrWhiteSpace())
            {
                ValidationManager.SetError("User should not be empty", nameof(ActivityUI.UsersList));
            }
            return Task.FromResult(0);
        }


        public override async Task Run()
        {
            if (ValidationManager.HasErrors)
            {
                RaiseError("Invalid input was selected/entered", ErrorType.Generic,
                    ActivityErrorCode.DESIGN_TIME_DATA_INVALID, MyTemplate.Name, MyTemplate.Terminal.Name);
            }
            else
            {
                var query = new AsanaTaskQuery()
                {
                    Workspace = ActivityUI.WorkspacesList.Value,
                    Assignee = ActivityUI.UsersList.Value,
                    Project = ActivityUI.ProjectsList.Value
                };

                var tasks = await _tasks.GetAsync(query);

                var payloadObjNames = tasks.Select(t => new KeyValueDTO("Task name", t.Name));
                var payloadObjIds = tasks.Select(t => new KeyValueDTO("Task id", t.Id));

                var payloadNames = Crate<KeyValueListCM>.FromContent(    RunTimeCrateLabel,
                                                                    new KeyValueListCM(payloadObjNames),
                                                                    AvailabilityType.Always);

                var payloadIds = Crate<KeyValueListCM>.FromContent(RunTimeCrateLabel,
                                                                    new KeyValueListCM(payloadObjIds),
                                                                    AvailabilityType.Always);

                Payload.Add(payloadNames);
                Payload.Add(payloadIds);
            }
        }                               
        
    }
}