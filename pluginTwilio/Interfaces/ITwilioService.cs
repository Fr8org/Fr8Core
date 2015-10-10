using System;

namespace pluginTwilio.Services
{
    public interface ITwilioService
    {
        void SendSms(String number, String messageBody);
    }
}