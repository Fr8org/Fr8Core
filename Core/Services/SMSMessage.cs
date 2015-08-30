using Core.Interfaces;
using Core.Managers.APIManagers.Packagers;
using StructureMap;

namespace Core.Services
{
    public class SMSMessage : ISMSMessage
    {
        private ISMSPackager _smsPackager;

        public SMSMessage()
        {
            _smsPackager = ObjectFactory.GetInstance<ISMSPackager>();
        }

        public void Send(string number, string message)
        {
            _smsPackager.SendSMS(number, message);
        }
    }
}
