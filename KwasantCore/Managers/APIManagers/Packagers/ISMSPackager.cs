using System;
using Twilio;

namespace KwasantCore.Managers.APIManagers.Packagers
{
    public interface ISMSPackager
    {
        SMSMessage SendSMS(String number, String message);
    }
}