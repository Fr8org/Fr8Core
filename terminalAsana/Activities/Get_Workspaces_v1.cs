using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using terminalAsana.Asana;
using terminalAsana.Asana.Services;
using terminalAsana.Interfaces;

namespace terminalAsana.Activities
{
    public class Get_Workspaces_v1 : TerminalActivity<Get_Workspaces_v1.ActivityUi>
    {
        private IAsanaOAuth _asanaOAuth;
        private IAsanaOAuthCommunicator _oAuthCommunicator;
        private IAsanaWorkspaces _workspaces;


        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Get_Workspaces",
            Label = "Get Workspaces",
            Category = ActivityCategory.Receivers,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string RunTimeCrateLabel = "Get Workspaces";
        private const string ResultFieldLabel = "ActivityResult";


        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList WorkspacesList;

            public ActivityUi()
            {
                WorkspacesList = new DropDownList()
                {
                    Label = "Avaliable workspaces",
                    Name = nameof(WorkspacesList),
                    ListItems = new List<ListItem>()
                };

                Controls = new List<ControlDefinitionDTO>(){ WorkspacesList};
            }
        }

        public Get_Workspaces_v1(ICrateManager crateManager, IAsanaOAuth oAuth) : base(crateManager)
        {
            _asanaOAuth = oAuth;
        }

        protected override void InitializeInternalState()
        {
            base.InitializeInternalState();
            _asanaOAuth = Task.Run(() => _asanaOAuth.InitializeAsync(this.AuthorizationToken)).Result;
            _oAuthCommunicator = new AsanaCommunicatorService(_asanaOAuth, new RestfulServiceClient());

            var asanaParams = new AsanaParametersService();
            _workspaces = new Workspaces(_oAuthCommunicator, asanaParams);
        }

        public override async Task Initialize()
        {
            ActivityUI.WorkspacesList.ListItems = _workspaces.GetAll().Select( w => new ListItem() { Key= w.Id, Value = w.Name} ).ToList();
        }

        public override async Task FollowUp()
        {
            
            
        }

        public override async Task Run()
        {
            
        }
        
    }
}