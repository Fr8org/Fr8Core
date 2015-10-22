using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;

namespace terminalTwilio.Services
{
    public interface ITwilioService
    {
        void SendSms(String number, String messageBody);
        IEnumerable<string> GetRegisteredSenderNumbers();
    }
}