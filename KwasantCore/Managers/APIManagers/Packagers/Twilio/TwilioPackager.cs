using System;
using KwasantCore.ExternalServices;
using StructureMap;
using Twilio;
using Utilities;

namespace KwasantCore.Managers.APIManagers.Packagers.Twilio
{
    public class TwilioPackager : ISMSPackager
    {
        private const string AccountSIDWebConfigName = "TWILIO_SID";
        private const string AuthTokenWebConfigName = "TWILIO_TOKEN";
        private const string FromNumberWebConfigName = "TwilioFromNumber";

        private readonly ITwilioRestClient _twilio;
        private readonly String _twilioFromNumber;
        public TwilioPackager()
        {
            var configRepo = ObjectFactory.GetInstance<IConfigRepository>();
            //this will be overridden by Azure settings with the same name, on RC, Staging, and Production
            string accountSID = configRepo.Get(AccountSIDWebConfigName);
            string accountAuthKey = configRepo.Get(AuthTokenWebConfigName);
            _twilioFromNumber = configRepo.Get(FromNumberWebConfigName);

            if (String.IsNullOrEmpty(accountSID))
                throw new ArgumentNullException(AccountSIDWebConfigName, @"Value must be set in web.config");

            if (String.IsNullOrEmpty(accountAuthKey))
                throw new ArgumentNullException(AuthTokenWebConfigName, @"Value must be set in web.config");

            if (String.IsNullOrEmpty(_twilioFromNumber))
                throw new ArgumentNullException(FromNumberWebConfigName, @"Value must be set in web.config");

            _twilio = ObjectFactory.GetInstance<ITwilioRestClient>();
            _twilio.Initialize(accountSID, accountAuthKey);
        }

        public SMSMessage SendSMS(String number, String message)
        {
            SMSMessage result = _twilio.SendSmsMessage(_twilioFromNumber, number, message);
            if (result.RestException != null)
            {
                if (result.RestException.MoreInfo == "https://www.twilio.com/docs/errors/21606" && System.Diagnostics.Debugger.IsAttached)
                {
                    //swallow the twilio exception that gets thrown when you use the test account, so it doesn't clutter up the logs
                }
                else
                {
                    throw new Exception(result.RestException.Message);
                }
            }
            return result;
        }
    }
}
