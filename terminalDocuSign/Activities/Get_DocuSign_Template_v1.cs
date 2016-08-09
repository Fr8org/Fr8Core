using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Activities
{
    public class Get_DocuSign_Template_v1 : BaseDocuSignActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("5E92E326-06E3-4C5B-A1F9-7542E8CD7C07"),
            Version = "1",
            Name = "Get_DocuSign_Template",
            Label = "Get DocuSign Template",
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        protected override string ActivityUserFriendlyName => "Get DocuSign Template";
        private string crateName = "DocuSign Template";

        public Get_DocuSign_Template_v1(ICrateManager crateManager, IDocuSignManager docuSignManager)
            : base(crateManager, docuSignManager)
        {
        }

        public override async Task Run()
        {
            //Get template Id
            var control = GetControl<DropDownList>("Available_Templates");
            string selectedDocusignTemplateId = control.Value;
            if (selectedDocusignTemplateId == null)
            {
                RaiseError("No Template was selected at design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                return;
            }
            var config = DocuSignManager.SetUp(AuthorizationToken);
            //lets download specified template from user's docusign account
            var downloadedTemplate = DocuSignManager.DownloadDocuSignTemplate(config, selectedDocusignTemplateId);
            //and add it to payload
            var templateCrate = CreateDocuSignTemplateCrateFromDto(downloadedTemplate);
            Payload.Add(templateCrate);
            Success();
        }

        private Crate CreateDocuSignTemplateCrateFromDto(JObject template)
        {
            var manifest = new DocuSignTemplateCM
            {
                Body = template.ToString(),
                CreateDate = DateTime.UtcNow,
                Name = template.SelectToken("envelopeTemplateDefinition.name").Value<string>()
            };

            return Crate.FromContent(crateName, manifest);
        }

        public override Task Initialize()
        {
            Storage.Clear();

            CreateControlsCrate();
            FillDocuSignTemplateSource("Available_Templates");
            
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            CrateSignaller.MarkAvailable<DocuSignTemplateCM>(crateName, AvailabilityType.RunTime);

            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            var templateList = GetControl<DropDownList>("Available_Templates");

            if (ValidationManager.ValidateControlExistance(templateList))
            {
                ValidationManager.ValidateTemplateList(templateList);
            }

            return Task.FromResult(0);
        }

        private void CreateControlsCrate()
        {
            var availableTemplates = new DropDownList
            {
                Label = "Get which template",
                Name = "Available_Templates",
                Value = null,
                Source = null,
                Events = new List<ControlEvent> { ControlEvent.RequestConfig },
            };

            AddControl(availableTemplates);
        }
    }
}