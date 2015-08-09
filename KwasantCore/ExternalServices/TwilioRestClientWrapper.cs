using System;
using Twilio;

namespace KwasantCore.ExternalServices
{
    public class TwilioRestClientWrapper : ITwilioRestClient
    {
        private TwilioRestClient _internalClient;
        private ServiceManager<TwilioRestClientWrapper> _serviceManager;

        public void Initialize(string accountSID, string accountAuthKey)
        {
            _serviceManager = new ServiceManager<TwilioRestClientWrapper>("Twilio Service", "SMS");
            _internalClient = new TwilioRestClient(accountSID, accountAuthKey);
        }

        public SMSMessage SendSmsMessage(string from, string to, string body)
        {
            _serviceManager.LogEvent("Sending an sms...");
            try
            {
                var message = _internalClient.SendSmsMessage(from, to, body);
                _serviceManager.LogSucessful("SMS sent.");
                return message;
            }
            catch (Exception ex)
            {
                _serviceManager.LogFail(ex, "Failed to send an sms.");    
                throw;
            }
            
        }
    }
}
