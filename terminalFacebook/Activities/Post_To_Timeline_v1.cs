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
            Id = new Guid("9710de37-7f5a-471a-9e94-c1ade0f71474"),
            Name = "Post_To_Timeline",
            Label = "Post To Timeline",
            Version = "1",
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextSource Message { get; set; }

            public ActivityUi()
            {
                Message = new TextSource
                {
                    InitialLabel = "Message",
                    Label = "Message",
                    Name = nameof(Message),
                    Source = new FieldSourceDTO
                    {
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                        RequestUpstream = true
                    }
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

        public override async Task Run()
        {
            await _fbIntegration.PostToTimeline(AuthorizationToken.Token, ActivityUI.Message.TextValue);
        }
    }
}