using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Services;
using terminalMailChimp.Interfaces;
using terminalMailChimp.Models;

namespace terminalMailChimp.Activities
{
    public class Update_List_v1 : TerminalActivity<Update_List_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("3029117d-1bca-43e1-b5ac-03778f6aeff6"),
            Name = "Update_List",
            Label = "Update List",
            Version = "1",
            Category = ActivityCategory.Forwarders,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            WebService = TerminalData.WebServiceDTO,
            Categories = new[]
            {
                ActivityCategories.Forward,
                new ActivityCategoryDTO(TerminalData.WebServiceDTO.Name, TerminalData.WebServiceDTO.IconPath)
            }
        };

        private const string MemberSubscribedStatus = "subscribed";

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private readonly IMailChimpIntegration _mailChimpIntegration;

        public Update_List_v1(ICrateManager crateManager, IMailChimpIntegration mailChimpIntegration) : base(crateManager)
        {
            _mailChimpIntegration = mailChimpIntegration;
        }

        private string SelectedMailChimpList
        {
            get { return this[nameof(SelectedMailChimpList)]; }
            set { this[nameof(SelectedMailChimpList)] = value; }
        }

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList MailChimpListSelector { get; set; }

            public TextSource MemberEmailAddress { get; set; }

            public TextSource MemberFirstName { get; set; }

            public TextSource MemberLastName { get; set; }

            public ActivityUi() : this(new UiBuilder()) { }

            public ActivityUi(UiBuilder uiBuilder)
            {
                MailChimpListSelector = new DropDownList
                {
                    Label = "Select a MailChimp List",
                    Name = nameof(MailChimpListSelector),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };

                MemberEmailAddress = uiBuilder.CreateSpecificOrUpstreamValueChooser("Member Email Address", nameof(MemberEmailAddress), CrateManifestTypes.StandardDesignTimeFields, requestUpstream: true, groupLabelText: "Add new Member to List");

                MemberFirstName = uiBuilder.CreateSpecificOrUpstreamValueChooser("Member First Name", nameof(MemberFirstName), CrateManifestTypes.StandardDesignTimeFields, requestUpstream: true, groupLabelText: "Add new Member to List");

                MemberLastName = uiBuilder.CreateSpecificOrUpstreamValueChooser("Member Last Name", nameof(MemberLastName), CrateManifestTypes.StandardDesignTimeFields, requestUpstream: true, groupLabelText: "Add new Member to List");
                    
                Controls = new List<ControlDefinitionDTO>() {  MailChimpListSelector, MemberEmailAddress, MemberFirstName, MemberLastName};
            }
        }

        public override async Task Initialize()
        {
            ActivityUI.MailChimpListSelector.ListItems = (await _mailChimpIntegration.GetLists(AuthorizationToken))
                .Select(x => new ListItem { Key = x.Name, Value = x.Id }).ToList();
        }

        public override async Task FollowUp()
        {
            if (!string.IsNullOrEmpty(ActivityUI.MailChimpListSelector.Value))
            {
                var previousGroup = SelectedMailChimpList;
                if (string.IsNullOrEmpty(previousGroup) || !string.Equals(previousGroup, ActivityUI.MailChimpListSelector.Value))
                {
                    var mailChimpLists = await _mailChimpIntegration.GetLists(AuthorizationToken);

                    ActivityUI.MailChimpListSelector.ListItems = mailChimpLists.Select(x => new ListItem { Key = x.Name, Value = x.Id }).ToList();

                    var firstList = mailChimpLists.FirstOrDefault();
                    if (firstList != null)
                    {
                        ActivityUI.MailChimpListSelector.SelectByValue(firstList.Id);
                    }
                }
                SelectedMailChimpList = ActivityUI.MailChimpListSelector.Value;
                ActivityUI.MailChimpListSelector.ListItems = (await _mailChimpIntegration.GetLists(AuthorizationToken))
                    .Select(x => new ListItem {Key = x.Name, Value = x.Id}).ToList();
                ActivityUI.MailChimpListSelector.Value = SelectedMailChimpList;
            }
            else
            {
                ActivityUI.MailChimpListSelector.ListItems.Clear();
                ActivityUI.MailChimpListSelector.selectedKey = string.Empty;
                ActivityUI.MailChimpListSelector.Value = string.Empty;
                SelectedMailChimpList = string.Empty;
            }
        }

        protected override Task Validate()
        {
            if (string.IsNullOrEmpty(ActivityUI.MailChimpListSelector.Value))
            {
                ValidationManager.SetError("MailChimp List is not specified", ActivityUI.MailChimpListSelector);
            }

            ValidationManager.ValidateTextSourceNotEmpty(ActivityUI.MemberEmailAddress, "Member email address is required");

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            await _mailChimpIntegration.UpdateListWithNewMember(AuthorizationToken, 
                new Member()
                {
                    EmailAddress = ActivityUI.MemberEmailAddress.TextValue,
                    Status = MemberSubscribedStatus,
                    ListId = ActivityUI.MailChimpListSelector.Value,
                    MergeFields = new MergeFields()
                    {
                        FirstName = ActivityUI.MemberFirstName.TextValue,
                        LastName = ActivityUI.MemberLastName.TextValue,
                    },
                });

            Success();
        }
    }
}