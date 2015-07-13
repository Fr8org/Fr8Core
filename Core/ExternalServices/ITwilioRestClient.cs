using System;
using Twilio;

namespace Core.ExternalServices
{
    public interface ITwilioRestClient
    {
        SMSMessage SendSmsMessage(String from, String to, String body);
        void Initialize(String accountSID, String accountAuthKey);
    }
}
