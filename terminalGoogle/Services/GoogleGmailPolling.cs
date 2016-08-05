using Fr8.Infrastructure.Interfaces;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services.Authorization;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using Newtonsoft.Json;
using Google.Apis.Discovery;
using Google.Apis.Gmail.v1.Data;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Crates;
using Fr8.TerminalBase.Services;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using Fr8.Infrastructure.Utilities.Configuration;

namespace terminalGoogle.Services
{
    public class GoogleGmailPolling : IGoogleGmailPolling
    {
        private IGoogleIntegration _googleIntegration;
        private IHubEventReporter _reporter;
        private static readonly ILog Logger = LogManager.GetLogger("terminalGoogle");

        public GoogleGmailPolling(IGoogleIntegration _googleIntegration, IHubEventReporter reporter)
        {
            this._googleIntegration = _googleIntegration;
            this._reporter = reporter;

        }

        public async Task SchedulePolling(IHubCommunicator hubCommunicator, string externalAccountId, bool trigger_immediately)
        {
            string pollingInterval = "1";
            await hubCommunicator.ScheduleEvent(externalAccountId, pollingInterval, trigger_immediately);
        }

        public async Task<PollingDataDTO> Poll(PollingDataDTO pollingData)
        {

            Logger.Info($"Polling for Gmail was launched {pollingData.ExternalAccountId}");

            var serv = new GoogleGmail();
            var googleAuthToken = JsonConvert.DeserializeObject<GoogleAuthDTO>(pollingData.AuthToken);
            var service = await serv.CreateGmailService(googleAuthToken);

            if (string.IsNullOrEmpty(pollingData.Payload))
            {
                //Polling is called for the first time
                //we have no history id to synchronise partitially, so we request the last message from the inbox
                UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List(pollingData.ExternalAccountId);
                request.RequestParameters["maxResults"] = new Parameter() { DefaultValue = "1", Name = "maxResults", ParameterType = "query" };
                var list = request.Execute();

                //then we have to get its details and historyId (to use with history listing API method)

                pollingData.Payload = GetHistoryId(service, list.Messages.FirstOrDefault().Id, pollingData.ExternalAccountId);
                Logger.Info($"Polling for Gmail {pollingData.ExternalAccountId}: remembered the last email in the inbox");

            }
            else
            {
                var request = service.Users.History.List(pollingData.ExternalAccountId);
                request.StartHistoryId = ulong.Parse(pollingData.Payload);
                var result = request.Execute();
                Logger.Info($"Polling for Gmail {pollingData.ExternalAccountId}: received a history of changes");
                if (result.History != null)
                    foreach (var historyRecord in result.History)
                    {
                        if (historyRecord.MessagesAdded != null)
                        {
                            foreach (var mail in historyRecord.MessagesAdded.Reverse())
                            {
                                //TODO: make a batch request for emails, instead of calling one by one
                                var email = GetEmail(service, mail.Message.Id, pollingData.ExternalAccountId);
                                var eventReportContent = new EventReportCM
                                {
                                    EventNames = "GmailInbox",
                                    EventPayload = new CrateStorage(Crate.FromContent("GmailInbox", email)),
                                    Manufacturer = "Google",
                                    ExternalAccountId = pollingData.ExternalAccountId
                                };

                                pollingData.Payload = email.MessageID;
                                Logger.Info("Polling for Gmail: got a new email, broadcasting an event to the Hub");
                                await _reporter.Broadcast(Crate.FromContent("Standard Event Report", eventReportContent));
                            }
                        }
                    }
                else Logger.Info($"Polling for Gmail {pollingData.ExternalAccountId}: no new emails");

            }
            pollingData.Result = true;
            return pollingData;
        }


        public string GetMimeString(MessagePart Parts)
        {
            string Body = "";

            if (Parts.Parts != null)
            {
                foreach (MessagePart part in Parts.Parts)
                {
                    Body = String.Format("{0}\n{1}", Body, GetMimeString(part));
                }
            }
            else if (Parts.Body.Data != null && Parts.Body.AttachmentId == null && (Parts.MimeType == "text/html" || Parts.MimeType == "text/plain"))
            {
                String codedBody = Parts.Body.Data.Replace("-", "+");
                codedBody = codedBody.Replace("_", "/");
                byte[] data = Convert.FromBase64String(codedBody);
                Body = Encoding.UTF8.GetString(data);
            }

            return Body;
        }


        private StandardEmailMessageCM GetEmail(GmailService service, string Id, string externalAccountId)
        {
            var result = new StandardEmailMessageCM();

            var get_request = service.Users.Messages.Get(externalAccountId, Id);
            get_request.RequestParameters["format"] = new Parameter() { Name = "format", DefaultValue = "full", ParameterType = "query" };
            var message_details = get_request.Execute();

            string from = message_details.Payload.Headers.Where(a => a.Name == "From").FirstOrDefault().Value;
            string subject = message_details.Payload.Headers.Where(a => a.Name == "Subject").FirstOrDefault().Value;

            var email = Regex.Match(from, "<.*>").Value.Trim();
            string name;
            if (email != "")
                name = from.Replace(email, "").Trim();
            else
                name = email;
            email = email.Trim(new char[] { '<', '>' }).Trim();
            result.DateReceived = message_details.Payload.Headers.Where(a => a.Name.Contains("Date")).FirstOrDefault().Value;
            result.EmailFrom = email;
            result.Subject = subject;
            result.EmailFromName = name;
            result.MessageID = message_details.HistoryId.ToString();
            string htmlText = "";
            if (message_details.Payload.Parts != null)
                htmlText = GetMimeString(message_details.Payload.Parts.Where(a => a.MimeType == "text/html" || a.MimeType == "text/plain").FirstOrDefault());
            else
                htmlText = Encoding.UTF8.GetString(Convert.FromBase64String(message_details.Payload.Body.Data.Replace('-', '+').Replace('_', '/')));

            result.HtmlText = htmlText;
            result.PlainText = GetPlainTextFromHtml(result.HtmlText);

            return result;
        }


        private string GetPlainTextFromHtml(string htmlString)
        {
            string htmlTagPattern = "<.*?>";
            var regexCss = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            htmlString = regexCss.Replace(htmlString, string.Empty);
            htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
            htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            htmlString = htmlString.Replace("&nbsp;", string.Empty);
            htmlString = Regex.Replace(htmlString, @"\s+", " ");

            return htmlString;
        }


        private string GetHistoryId(GmailService service, string messageId, string externalAccountId)
        {
            var get_request = service.Users.Messages.Get(externalAccountId, messageId);
            get_request.RequestParameters["format"] = new Parameter() { Name = "format", DefaultValue = "minimal", ParameterType = "query" };
            var message_details = get_request.Execute();
            return message_details.HistoryId.Value.ToString();
        }
    }
}