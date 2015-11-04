using System;
using System.Net;
using System.Net.Mail;

namespace Hub.ExternalServices
{
    public interface ISmtpClient
    {
        void Initialize(String serverURL, int serverPort);
        bool EnableSsl { get; set; }
        bool UseDefaultCredentials { get; set; }
        ICredentialsByHost Credentials { get; set; }
        void Send(MailMessage message);
    }
}
