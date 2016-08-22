using System;
using System.Net;
using System.Net.Mail;

namespace Hub.ExternalServices
{
    public class SmtpClientWrapper : ISmtpClient
    {
        private SmtpClient _internalClient;
        private ServiceManager<SmtpClientWrapper> _serviceManager;
        public void Initialize(string serverURL, int serverPort)
        {
            _serviceManager = new ServiceManager<SmtpClientWrapper>("SMTP Service: " + serverURL + ":" + serverPort, "Email Services");

            _internalClient = new SmtpClient(serverURL, serverPort);
        }

        public bool EnableSsl { get { return _internalClient.EnableSsl; } set { _internalClient.EnableSsl = value; } }
        public bool UseDefaultCredentials { get { return _internalClient.UseDefaultCredentials; } set { _internalClient.UseDefaultCredentials = value; } }
        public ICredentialsByHost Credentials { get { return _internalClient.Credentials; } set { _internalClient.Credentials = value; } }
        public void Send(MailMessage message)
        {
            _serviceManager.LogEvent("Sending an email...");
            try
            {
                _internalClient.Send(message);
                _serviceManager.LogSucessful("Email sent.");
            }
            catch (Exception ex)
            {
                _serviceManager.LogFail(ex, "Failed to send email.");
                throw;
            }
        }
    }
}
