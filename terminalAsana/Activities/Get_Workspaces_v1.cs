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

namespace terminalAsana.Activities
{
    public class Get_Workspaces_v1 : TerminalActivity<Get_Workspaces_v1.ActivityUi>
    {
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
            }
        }

        public Get_Workspaces_v1(ICrateManager crateManager) : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public override Task Run()
        {
            return Task.FromResult(0);
        }
        
    }
}