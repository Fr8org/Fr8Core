using System;
using System.Diagnostics;
using Core.ExternalServices;
using Twilio;
using fr8.Microsoft.Azure;

namespace pluginTwilio.Services
{
    public class TwilioService : ITwilioService
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        private const string AccountSidWebConfigName = "TwilioSid";
        private const string AuthTokenWebConfigName = "TwilioToken";
        private const string FromNumberWebConfigName = "TwilioFromNumber";

        /**********************************************************************************/

        private readonly String _twilioFromNumber;
        private readonly TwilioRestClient _internalClient;
        private readonly ServiceManager<TwilioService> _serviceManager;

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public TwilioService()
        {
            string accountSid = CloudConfigurationManager.GetSetting(AccountSidWebConfigName);
            string accountAuthKey = CloudConfigurationManager.GetSetting(AuthTokenWebConfigName);

            _twilioFromNumber = CloudConfigurationManager.GetSetting(FromNumberWebConfigName);

            if (String.IsNullOrEmpty(accountSid))
                throw new ArgumentNullException(AccountSidWebConfigName, @"Value must be set in web.config");

            if (String.IsNullOrEmpty(accountAuthKey))
                throw new ArgumentNullException(AuthTokenWebConfigName, @"Value must be set in web.config");

            if (String.IsNullOrEmpty(_twilioFromNumber))
                throw new ArgumentNullException(FromNumberWebConfigName, @"Value must be set in web.config");


            _serviceManager = new ServiceManager<TwilioService>("Twilio Service", "SMS");
            _internalClient = new TwilioRestClient(accountSid, accountAuthKey);
        }

        /**********************************************************************************/

        public void SendSms(String number, String messageBody)
        {
            var result = SendMessage(_twilioFromNumber, number, messageBody);
     
            if (result.RestException != null)
            {
               throw new Exception(result.RestException.Message);
            }
        }

        /**********************************************************************************/

        public Message SendMessage(string from, string to, string body)
        {
            _serviceManager.LogEvent("Sending an sms...");
            
            try
            {
                var message = _internalClient.SendMessage(from, to, body);
                _serviceManager.LogSucessful("Sms sent.");
                return message;
            }
            catch (Exception ex)
            {
                _serviceManager.LogFail(ex, "Failed to send an sms.");
                throw new Exception(ex.Message);
            }
        }

        /**********************************************************************************/
    }
}
