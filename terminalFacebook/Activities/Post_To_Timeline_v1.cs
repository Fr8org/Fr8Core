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
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using StructureMap;
using terminalFacebook.Interfaces;

namespace terminalFacebook.Activities
{
    public class Post_To_Timeline_v1 : TerminalActivity<Post_To_Timeline_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Post_To_Timeline",
            Label = "Post To Timeline",
            Category = ActivityCategory.Forwarders,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBox Message { get; set; }

            public ActivityUi()
            {
                Message = new TextBox
                {
                    Name = nameof(Message),
                    Label = "Post Message"
                };
                Controls = new List<ControlDefinitionDTO> { Message };
            }
        }

        private readonly IFacebookIntegration _fbIntegration;

        public Post_To_Timeline_v1(ICrateManager crateManager, IFacebookIntegration fbIntegration)
            : base(crateManager)
        {
            _fbIntegration = fbIntegration;
        }

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            
            return Task.FromResult(0);
        }

        public override Task Run()
        {
            _fbIntegration.PostToTimeline(AuthorizationToken.Token, ActivityUI.Message.Value);
            return Task.FromResult(0);
        }
    }
}