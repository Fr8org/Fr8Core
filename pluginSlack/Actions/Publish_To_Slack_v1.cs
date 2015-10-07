using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Text;
using terminal_base.BaseClasses;

namespace terminal_Slack.Actions
{

    public class Publish_To_Slack_v1 : BaseTerminalAction
    {

   
        public object Execute(SlackPayloadDTO curSlackPayload)
        {
            string responseText = string.Empty;
            Encoding encoding = new UTF8Encoding();

            const string webhookUrl = "WebhookUrl";
            Uri uri = new Uri(ConfigurationManager.AppSettings[webhookUrl]);

            string payloadJson = JsonConvert.SerializeObject(curSlackPayload);

            using (WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data["payload"] = payloadJson;

                var response = client.UploadValues(uri, "POST", data);

                responseText = encoding.GetString(response);
            }
            return responseText;
        }

    }
}