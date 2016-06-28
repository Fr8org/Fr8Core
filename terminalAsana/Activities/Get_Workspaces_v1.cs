using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using terminalAsana.Interfaces;

namespace terminalAsana.Activities
{
    public class Get_Workspaces_v1 : TerminalActivity<Get_Workspaces_v1.ActivityUi>
    {
        private IAsanaOAuth _asanaOAuth;

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
            public TextBlock Annotation;
            public DropDownList WorkspacesList;

            public ActivityUi()
            {
                Annotation = new TextBlock()
                {
                    Label = "Annotation",
                    Name = nameof(Annotation)
                };

                WorkspacesList = new DropDownList()
                {
                    Label = "Avaliable workspaces",
                    Name = nameof(WorkspacesList),
                    ListItems = new List<ListItem>()
                    {
                        new ListItem() { Key = "1", Selected = true, Value = "1"},
                        new ListItem() { Key = "2", Value = "2"}
                    }
                };

                Controls = new List<ControlDefinitionDTO>(){ Annotation, WorkspacesList};
            }
        }

        public Get_Workspaces_v1(ICrateManager crateManager, IAsanaOAuth oAuth) : base(crateManager)
        {
            _asanaOAuth = oAuth;
            
        }

        protected override void InitializeInternalState()
        {
            base.InitializeInternalState();
            _asanaOAuth = Task.Run(() => _asanaOAuth.InitializeAsync(this.AuthorizationToken, this.HubCommunicator)).Result;
        }

        public override async Task Initialize()
        {
            //_asanaOAuth = await _asanaOAuth.InitializeAsync(this.AuthorizationToken, this.HubCommunicator);
            var t = "";
        }

        public override async Task FollowUp()
        {
            //_asanaOAuth = await _asanaOAuth.InitializeAsync(this.AuthorizationToken, this.HubCommunicator);
            var t = "";
        }

        public override async Task Run()
        {
            //_asanaOAuth = await _asanaOAuth.InitializeAsync(this.AuthorizationToken, this.HubCommunicator);
            //return Task.FromResult(0);
        }
        
    }
}