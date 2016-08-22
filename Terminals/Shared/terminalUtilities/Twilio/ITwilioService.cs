using System;
using System.Collections.Generic;
using Twilio;

namespace terminalUtilities.Twilio
{
    public interface ITwilioService
    {
        Message SendSms(String number, String messageBody);
        IEnumerable<string> GetRegisteredSenderNumbers();
    }
}