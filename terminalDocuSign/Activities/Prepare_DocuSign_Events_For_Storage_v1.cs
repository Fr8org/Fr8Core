using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Activities
{
    public class Prepare_DocuSign_Events_For_Storage_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new System.Guid("ba1424f9-a540-4b3e-b206-695857011a0a"),
            Name = "Prepare_DocuSign_Events_For_Storage",
            Label = "Prepare DocuSign Events For Storage",
            Version = "1",
            Category = ActivityCategory.Monitors,
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            Tags = Tags.Internal,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Monitor,
                new ActivityCategoryDTO(TerminalData.WebServiceDTO.Name, TerminalData.WebServiceDTO.IconPath)
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public Prepare_DocuSign_Events_For_Storage_v1(ICrateManager crateManager)
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            Storage.Clear();

            /*
             * Discussed with Alexei and it is required to have empty Standard UI Control in the crate.
             * So we create a text block which informs the user that this particular aciton does not require any configuration.
             */
            var textBlock = UiBuilder.GenerateTextBlock("Monitor All DocuSign events", "This Action doesn't require any configuration.", "well well-lg");
            
            AddControl(textBlock);

            //create a Standard Event Subscription crate
            var curEventSubscriptionsCrate = CrateManager.CreateStandardEventSubscriptionsCrate("Standard Event Subscription", "DocuSign", DocuSignEventNames.GetAllEventNames());

            var authToken = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(AuthorizationToken.Token);
            var docuSignUserCrate = Crate.FromContent("DocuSignUserCrate", new StandardPayloadDataCM(new KeyValueDTO("DocuSignUserEmail", authToken.Email)));
            
            Storage.Add(curEventSubscriptionsCrate, docuSignUserCrate);

            CrateSignaller.MarkAvailableAtRuntime<DocuSignEnvelopeCM_v2>("DocuSign Envelope");

            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            Debug.WriteLine($"Running PrepareDocuSignEventForStorage: {ActivityId} - view viewhere!!!{0} - label {ActivityPayload.Label}");
            Debug.WriteLine($"for container {ExecutionContext.ContainerId} and authToken {AuthorizationToken}");

            var curEventReport = Payload.CrateContentsOfType<EventReportCM>().First();
            if (curEventReport.EventNames.Contains("Envelope"))
            {
                var crate = curEventReport.EventPayload.CrateContentsOfType<DocuSignEnvelopeCM_v2>().First();
                Payload.Add(Crate.FromContent("DocuSign Envelope", crate));
            }
            Debug.WriteLine($"Returning success for payload {ExecutionContext.ContainerId} - {Payload}");
            Success();
        }

        
    }
}