using Data.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using UtilitiesTesting;

namespace terminalDocuSign.Tests.Infrastructure
{
    public class IndexManagerTests : BaseTest
    {
        IndexManager _indexManager;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _indexManager = new IndexManager();          
        }
           
        [Test]
        public void GetEnvelopesTest()
        {
            DocuSignEnvelope test = new DocuSignEnvelope();            
            var accountEnvelopes = test.GetEnvelopes( DateTime.Now.AddDays(-1), DateTime.Now, true, 100);
            Assert.IsNotNull(accountEnvelopes);
        }

    }
}