using Hub.Managers.APIManagers.Packagers.SendGrid;

namespace Daemons.EventExposers
{
    public sealed class SendGridPackagerEventHandler : ExposedEvent
    {
        public static ExposedEvent EmailSent = new SendGridPackagerEventHandler("EmailSent");
        public static ExposedEvent EmailRejected = new SendGridPackagerEventHandler("EmailRejected");
        public static ExposedEvent EmailCriticalError = new SendGridPackagerEventHandler("EmailCriticalError");
        private SendGridPackagerEventHandler(string name)
            : base(name, typeof(SendGridPackager))
        {
        }
    }
}
