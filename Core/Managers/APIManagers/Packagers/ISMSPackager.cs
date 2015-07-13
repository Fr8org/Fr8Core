using System;
using Twilio;

namespace Core.Managers.APIManagers.Packagers
{
    public interface ISMSPackager
    {
        SMSMessage SendSMS(String number, String message);
    }
}