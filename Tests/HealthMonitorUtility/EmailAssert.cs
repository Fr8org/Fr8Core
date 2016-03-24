using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenPop.Pop3;
using System.Configuration;
using OpenPop.Mime;
using OpenPop.Mime.Header;
using System.Diagnostics;

namespace HealthMonitor.Utility
{
    public static class EmailAssert
    {
        // If a matching message has been received at most 60 seconds before calling the method, 
        // the test is considered passed.
        public static TimeSpan RecentMsgThreshold = new TimeSpan(0, 0, 60); // 60 seconds
        public static TimeSpan _timeout = new TimeSpan(0, 0, 30); // 30 seconds
        static bool _initialized = false;

        static string _testEmail;
        static string _hostname;
        static int _port;
        static bool _useSsl;
        static string _username;
        static string _password;

        public static void InitEmailAssert(string testEmail, string hostname, int port, bool useSsl, string username, string password)
        {
            Debug.AutoFlush = true;

            _testEmail = testEmail;
            _hostname = hostname;
            _port = port;
            _useSsl = useSsl;
            _username = username;
            _password = password;
            _initialized = true;
        }

        public static void EmailReceived(string expectedFromAddr, string expectedSubject, bool deleteMailOnSuccess = false)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Call Assert.InitEmailAssert(...) first.");
            }
            Debug.WriteLine("Expected Address: " + expectedFromAddr + ", Suject: " + expectedSubject);

            DateTime methodCalledTime = DateTime.UtcNow;

            using (Pop3Client client = new Pop3Client())
            {
                client.Connect(_hostname, _port, _useSsl);
                client.Authenticate(_username, _password);

                DateTime timeToCompare = methodCalledTime; // First time use method call time to compare time
                while (DateTime.UtcNow < methodCalledTime + _timeout)
                {
                    if (CheckEmail(client, expectedFromAddr, expectedSubject, timeToCompare, deleteMailOnSuccess))
                    {
                        client.Disconnect();
                        return;
                    }
                    System.Threading.Thread.Sleep(5000);
                    timeToCompare = DateTime.UtcNow; // Next time use current iteration call time 
                }
                throw new AssertionException(String.Format(
                        "Email to {0} was not received within the timeout period {1}.", 
                        _testEmail,
                        _timeout.ToString()));
            }
        }

        private static bool CheckEmail(Pop3Client client, string expectedFromAddr, string expectedSubject, DateTime startTime, bool deleteMailOnSuccess = false)
        {
            MessageHeader msg = null;
            int messageCount = client.GetMessageCount();
            for (int i = messageCount; i > 0; i--)
            {
                msg = client.GetMessageHeaders(i);
                if (ValidateTime(RecentMsgThreshold, startTime, msg.DateSent))
                {
                    if (ValidateConditions(expectedFromAddr, expectedSubject, msg))
                    {
                        if(deleteMailOnSuccess)
                        {
                            client.DeleteMessage(i);
                        }
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private static bool ValidateConditions(string expectedFromAddr, string expectedSubject, MessageHeader msg)
        {
            return string.Equals(expectedFromAddr, msg.From.Address, StringComparison.InvariantCultureIgnoreCase) 
                && string.Equals(expectedSubject, msg.Subject, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool ValidateTime(TimeSpan recentMsgThreshold, DateTime startTime, DateTime dateSent)
        {
            return dateSent >= startTime - recentMsgThreshold;
        }
    }
}
