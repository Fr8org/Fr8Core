using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalDocuSign.Services;

namespace terminalDocuSign.Tests.Infrastructure
{
    public class IndexManagerTests
    {
        [Test]
        public void GetEnvelopesTest()
        {
            DocuSignEnvelope test = new DocuSignEnvelope();
            string[] searchTypes = { "drafts", "awaiting_my_signature", "completed", "out_for_signature" };
            foreach (string type in searchTypes)
            {
                var accountEnvelopes = test.GetEnvelopes(type, DateTime.Now.AddMonths(-1), DateTime.Now, true, 1);
            }
        }

    }
}