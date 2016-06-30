using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.BaseClasses;
using terminalAsana.Asana;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;

namespace terminalAsana.Activities
{
    public class Get_Tasks_v1 : AsanaOAuthBaseActivity<Get_Tasks_v1.ActivityUi>
    {
        private IAsanaWorkspaces _workspaces;
        private IAsanaUsers _users;

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
                    ListItems = new List<ListItem>()
                };
                UsersList = new DropDownList()
                {
                    Label = "Users in workspace",
                    Name = nameof(UsersList),
                    ListItems = new List<ListItem>()
                };

                Controls = new List<ControlDefinitionDTO>(){ WorkspacesList};
            }


        }

        public Get_Tasks_v1(ICrateManager crateManager, IAsanaOAuth oAuth, IRestfulServiceClient client) : base(crateManager, oAuth, client)
        {
        }

        protected override void InitializeInternalState()
        {
            base.InitializeInternalState();

            var asanaParams = new AsanaParametersService();
            _workspaces = new Workspaces(OAuthCommunicator, asanaParams);
            _users = new Users(OAuthCommunicator, asanaParams);
        }

        public override async Task Initialize()
        {
            var items = _workspaces.GetAll();
            ActivityUI.WorkspacesList.ListItems = items.Select( w => new ListItem() { Key= w.Name, Value = w.Id} ).ToList();
            
            CrateSignaller.MarkAvailableAlways<StandardPayloadDataCM>(RunTimeCrateLabel).AddField("workspace_id")
                .AddField("workspace name");
        }

        public override async Task FollowUp()
        {     
            
        }

        public override async Task Run()
        {
            
        }
        
    }
}