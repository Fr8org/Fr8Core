using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using S22.Imap;
using StructureMap;
using Data.Infrastructure;
using Hub.ExternalServices;
using Hub.Interfaces;
using Hub.Managers;
using Utilities;
using Utilities.Logging;
using IImapClient = Hub.ExternalServices.IImapClient;

namespace Daemons
{
    public class InboundEmail : Daemon<InboundEmail>
    {
        private IImapClient _client;
        private readonly IConfigRepository _configRepository;
        private HashSet<String> _ignoreEmailsFrom;

        private readonly HashSet<String> _testSubjects = new HashSet<string>(); 
        public void RegisterTestEmailSubject(String subject)
        {
            lock (_testSubjects)
                _testSubjects.Add(subject);
        }

        public static event EventHandler<TestMessageReceivedEventArgs> TestMessageReceived;

        private string _fromEmailAddress;

        //warning: if you remove this empty constructor, Activator calls to this type will fail.
        public InboundEmail()
        {
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            _intakeManager = ObjectFactory.GetInstance<IIntakeManager>();
            _ignoreEmailsFrom = new HashSet<string>();
            var ignoreEmailsString = _configRepository.Get("IgnoreEmailsFrom", String.Empty);
            if (!String.IsNullOrWhiteSpace(ignoreEmailsString))
            {
                foreach (var emailToIgnore in ignoreEmailsString.Split(','))
                    _ignoreEmailsFrom.Add(emailToIgnore);
            }


            _fromEmailAddress = _configRepository.Get("EmailAddress_GeneralInfo");
            AddTest("OutboundEmailDaemon_Test", "Test");
        }

        private string GetIMAPServer()
        {
            return _configRepository.Get("InboundEmailHost");
        }

        private int GetIMAPPort()
        {
            return _configRepository.Get<int>("InboundEmailPort");
        }

        public String UserName;
        public string GetUserName()
        {
            return UserName ?? _configRepository.Get("INBOUND_EMAIL_USERNAME");
        }

        public String Password;
        private string GetPassword()
        {
            return Password ?? _configRepository.Get("INBOUND_EMAIL_PASSWORD");
        }

        private bool UseSSL()
        {
            return _configRepository.Get<bool>("InboundEmailUseSSL");
        }

        public override int WaitTimeBetweenExecution
        {
            get
            {
                return (int)TimeSpan.FromMinutes(2).TotalMilliseconds;
            }
        }

        private bool _alreadyListening;
        private readonly object _alreadyListeningLock = new object();
        private readonly IIntakeManager _intakeManager;

        protected override void Run()
        {
            LogEvent();
            lock (_alreadyListeningLock)
            {
                if (!_alreadyListening)
                {
                    _client = ObjectFactory.GetInstance<IImapClient>();
                    _client.Initialize(GetIMAPServer(), GetIMAPPort(), UseSSL());

                    string curUser = GetUserName();
                    string curPwd = GetPassword();
                    _client.Login(curUser, curPwd, AuthMethod.Login);

                    LogEvent("Waiting for messages at " + GetUserName() + "...");
                    _client.NewMessage += OnNewMessage;
                    _client.IdleError += OnIdleError;

                    _alreadyListening = true;
                }
            }
            GetUnreadMessages(_client);
        }

        private void OnIdleError(object sender, IdleErrorEventArgsWrapper args)
        {
            //Instead of logging it as an error - log it as an event. This doesn't mean we've lost any emails, so there's no reason to reduce success %.
            //This happens often on Kwasant - yet to be diagnosed.
            var eventName = "Idle error recieved.";
            var currException = args.Exception;
            var exceptionMessages = new List<String>();
            while (currException != null)
            {
                exceptionMessages.Add(currException.Message);
                currException = currException.InnerException;
            }
            exceptionMessages.Add("*** Stacktrace ***");
            exceptionMessages.Add(args.Exception.StackTrace);

            var exceptionMessage = String.Join(Environment.NewLine, exceptionMessages);

            eventName += " " + exceptionMessage;
            LogEvent(eventName);

            RestartClient();
        }

        public void OnNewMessage(object sender, IdleMessageEventArgsWrapper args)
        {
            LogEvent("New email notification received.");
            GetUnreadMessages(args.Client);
        }

        private void RestartClient()
        {
            lock (_alreadyListeningLock)
            {
                LogEvent("Restarting...");

                CleanUp();
            }
        }
        
        private void GetUnreadMessages(IImapClient client)
        {
            try
            {
                LogEvent("Querying for messages...");
                var messages = client.GetMessages(client.Search(SearchCondition.Unseen())).ToList();
                LogSuccess(messages.Count + " messages received.");

                foreach (var message in messages)
                {
                    bool deleteMessage;
                    ProcessMessageInfo(message, out deleteMessage);
                    if (deleteMessage)
                    {
                        try
                        {
                            client.DeleteMessages(client.Search(SearchCondition.Header("Message-ID", message.Headers["Message-ID"])));
                        }
                        catch (Exception e)
                        {
                            //Logger.GetLogger().Warn("Unable to delete a test message", e);
                            Logger.LogWarning($"Unable to delete a test message. Exception = {e}");
                        }
                    }
                }
            }
            catch (SocketException ex) //we were getting strange socket errors after time, and it looks like a reset solves things
            {
                EventManager.EmailProcessingFailure(DateTime.UtcNow.to_S(), "Got that SocketException");
                LogFail(ex, "Hit SocketException. Trying to reset the IMAP Client.");
                RestartClient();
            }
            catch (Exception e)
            {
                LogFail(e);
                RestartClient();
            }
        }

        private void ProcessMessageInfo(MailMessage messageInfo, out bool deleteMessage)
        {
            deleteMessage = false;
            var logString = "Processing message with subject '" + messageInfo.Subject + "'";
            //Logger.GetLogger().Info(logString);
            Logger.LogInfo(logString);
            LogEvent(logString);

            lock (_testSubjects)
            {
                if (_testSubjects.Contains(messageInfo.Subject))
                {
                    LogEvent("Test message detected.");
                    _testSubjects.Remove(messageInfo.Subject);

                    if (TestMessageReceived != null)
                    {
                        var args = new TestMessageReceivedEventArgs(messageInfo.Subject);
                        TestMessageReceived(this, args);
                        if (args.DeleteFromInbox)
                        {
                            deleteMessage = true;
                        }
                        LogSuccess();
                    }
                    else
                        LogFail(new Exception("No one was listening for test message event..."));

                    return;
                }
            }

            if (FilterUtility.IsReservedEmailAddress(messageInfo.From.Address))
            {
                LogEvent("Email ignored from " + messageInfo.From.Address);
                return;
            }

            try
            {
                _intakeManager.AddEmail(messageInfo);
            }
            catch (Exception e)
            {
                EventManager.EmailProcessingFailure(messageInfo.Headers["Date"], e.Message);
                LogFail(e, String.Format("EmailProcessingFailure Reported. ObjectID = {0}", messageInfo.Headers["Message-ID"]));
            }
        }

        protected override void CleanUp()
        {
            if (_client != null)
            {
                _client.NewMessage -= OnNewMessage;
                _client.IdleError -= OnIdleError;
                _client.Dispose();
                _client = null;
            }

            LogEvent("Shutting down...");
            _alreadyListening = false;
        }
    }

    public class TestMessageReceivedEventArgs : EventArgs
    {
        public string Subject { get; private set; }
        public bool DeleteFromInbox { get; set; }

        public TestMessageReceivedEventArgs(string subject)
        {
            Subject = subject;
        }
    }

}
