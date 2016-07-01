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

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Get_Tasks",
            Label = "Get Tasks",
            Category = ActivityCategory.Receivers,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        

        private const string RunTimeCrateLabel = "Get Tasks";
        private const string ResultFieldLabel = "ActivityResult";


        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList WorkspacesList;
            public DropDownList UsersList;
            
            public ActivityUi()
            {
                WorkspacesList = new DropDownList()
                {
                    Label = "Avaliable workspaces",
                    Name = nameof(WorkspacesList),
                    ListItems = new List<ListItem>(),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                };
                UsersList = new DropDownList()
                {
                    Label = "Users in workspace",
                    Name = nameof(UsersList),
                    ListItems = new List<ListItem>(),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                Controls = new List<ControlDefinitionDTO>(){ WorkspacesList, UsersList };
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
        }

        public override async Task Initialize()
        {
            var workspaces = _workspaces.GetAll();
            ActivityUI.WorkspacesList.ListItems = workspaces.Select( w => new ListItem() { Key= w.Name, Value = w.Id} ).ToList();

            CrateSignaller.MarkAvailableAlways<StandardPayloadDataCM>(RunTimeCrateLabel)
                .AddField(new FieldDTO("Task name", AvailabilityType.Always))
                .AddField(new FieldDTO("Task id",AvailabilityType.Always));

        }

        public override async Task FollowUp()
        {
            if (!ActivityUI.WorkspacesList.Value.IsNullOrWhiteSpace())
            {
                var users =  await _users.GetUsersAsync(ActivityUI.WorkspacesList.Value);
                ActivityUI.UsersList.ListItems = users.Select(w => new ListItem() {Key = w.Name, Value = w.Id}).ToList();
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
                ValidationManager.SetError("Task should not be empty", nameof(ActivityUI.UsersList));
             
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
                    Assignee = ActivityUI.UsersList.Value
                };

                var tasks = await _tasks.GetAsync(query);

                var payloadObj = tasks.Select(t => new KeyValueDTO("Task name", t.Name));
                

                //foreach (var task in tasks)
                //{
                //    var taskName = new KeyValueDTO("Task name",task.Name);

                //}

                var payload = Crate<StandardPayloadDataCM>.FromContent(    RunTimeCrateLabel,
                                                                    new StandardPayloadDataCM(payloadObj),
                                                                    AvailabilityType.Always);

                Payload.Add(payload);
            }


        }                               
        
    }
}