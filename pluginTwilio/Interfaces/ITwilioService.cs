using System;
using System.Collections.Generic;

namespace pluginTwilio.Services
{
    public interface ITwilioService
    {
        void SendSms(String number, String messageBody);
        IEnumerable<string> GetRegisteredSenderNumbers();
    }
}