using System;
using System.Collections.Generic;
using System.Net.Mail;
using S22.Imap;

namespace Hub.ExternalServices
{
    public interface IImapClient
    {
        void Initialize(String serverURL, int port, bool useSSL);
        void Login(string username, string password, AuthMethod method);
        IEnumerable<MailMessage> GetMessages(IEnumerable<uint> uids, bool seen = true, string mailbox = null);
        IEnumerable<uint> Search(SearchCondition criteria, string mailbox = null);
        void Dispose();
        event EventHandler<IdleMessageEventArgsWrapper> NewMessage;
        event EventHandler<IdleErrorEventArgsWrapper> IdleError;
        void DeleteMessages(IEnumerable<uint> uid);
    }

    public class IdleMessageEventArgsWrapper
    {
        public IImapClient Client { get; private set; }

        public IdleMessageEventArgsWrapper(IImapClient client)
        {
            Client = client;
        }
    }

    public class IdleErrorEventArgsWrapper
    {
        public IImapClient Client { get; private set; }
        public Exception Exception { get; private set; }

        public IdleErrorEventArgsWrapper(IImapClient client, Exception exception)
        {
            Client = client;
            Exception = exception;
        }
    }
}
