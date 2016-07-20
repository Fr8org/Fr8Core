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
            Category = ActivityCategory.Receivers,
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Receive,
                new ActivityCategoryDTO(TerminalData.WebServiceDTO.Name, TerminalData.WebServiceDTO.IconPath)
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        protected override string ActivityUserFriendlyName => "Get DocuSign Template";


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
                Body = JsonConvert.SerializeObject(template),
                CreateDate = DateTime.UtcNow,
                Name = template["Name"].ToString(),
                Status = template.Property("Name").SelectToken("status").Value<string>()
            };

            return Crate.FromContent("DocuSign Template", manifest);
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