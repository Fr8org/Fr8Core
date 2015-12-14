using System;
using System.Collections.Generic;
using Twilio;

namespace terminalTwilio.Services
{
    public interface ITwilioService
    {
        Message SendSms(String number, String messageBody);
        IEnumerable<string> GetRegisteredSenderNumbers();
    }
}