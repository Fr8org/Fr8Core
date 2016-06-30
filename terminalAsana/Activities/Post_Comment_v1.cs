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
using Microsoft.Ajax.Utilities;
using terminalAsana.Interfaces;

namespace terminalAsana.Activities
{
    public class Post_Comment_v1 : AsanaOAuthBaseActivity<Post_Comment_v1.ActivityUi>
    {
        private IAsanaWorkspaces    _workspaces;
        private IAsanaTasks         _tasks;


        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Post_Comment",
            Label = "Post Comment",
            Category = ActivityCategory.Forwarders,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true            
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string RunTimeCrateLabel = "Post Comment";
        private const string ResultFieldLabel = "ActivityResult";


        public class ActivityUi : StandardConfigurationControlsCM
        {
            
            public DropDownList Workspaces { get; set; }
            public DropDownList Tasks { get; set; }
            public TextSource   Comment { get; set; }

            public ActivityUi()
            {
                Workspaces = new DropDownList
                {
                    Label = "Select Workspace",
                    Name = nameof(Workspaces),
                    Required = true,
                    ListItems = new List<ListItem>(),
                    
                };
                Tasks = new DropDownList
                {
                    Label = "Select Task",
                    Name = nameof(Tasks),
                    Required = true,
                    ListItems = new List<ListItem>(),
                };
                Comment = new TextSource
                {
                    InitialLabel = "Comment",
                    Label = "Comment",
                    Name = nameof(Comment),
                    Source = new FieldSourceDTO
                    {
                        ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                        RequestUpstream = true
                    }
                };
                Controls = new List<ControlDefinitionDTO> { Workspaces, Tasks, Comment };
            }
        }

        public Post_Comment_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }



        public override Task Initialize()
        {
            var workspaces = 

            //var resultField = new FieldDTO(ResultFieldLabel, AvailabilityType.RunTime);
            //CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel, true).AddField(resultField);
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            if (ActivityUI.Workspaces.selectedKey == null)
            {
                ValidationManager.SetError("No workspace was selected", nameof(ActivityUI.Workspaces));
            }

            if (ActivityUI.Tasks.selectedKey == null)
            {
                ValidationManager.SetError("No task was selected", nameof(ActivityUI.Tasks));
            }

            if (!ActivityUI.Comment.HasValue)
            {
                ValidationManager.SetError("No data was entered for Comment", nameof(ActivityUI.Comment));
            }
            
            return Task.FromResult(0);
        }

        protected bool IsValid()
        {
            var isCommentValid = !ActivityUI.Comment.GetValue(Payload).IsNullOrWhiteSpace();
            if (!isCommentValid)
            {
                ValidationManager.SetError("Invalid data entered for Comment", nameof(ActivityUI.Comment));
                return false;
            }

            return true;
        }

        public override Task Run()
        {
            if (!IsValid())
            {
                RaiseError("Invalid input was selected/entered", ErrorType.Generic,
                    ActivityErrorCode.DESIGN_TIME_DATA_INVALID, MyTemplate.Name, MyTemplate.Terminal.Name);

            }
            else
            {
                

                //var resultField = new KeyValueDTO(ResultFieldLabel, result.ToString(CultureInfo.InvariantCulture));
                //var resultCrate = Crate.FromContent(RunTimeCrateLabel, new StandardPayloadDataCM(resultField));
                //Payload.Add(resultCrate);
                
            }
            return Task.FromResult(0);
        }
    }
}