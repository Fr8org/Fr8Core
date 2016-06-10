using Data.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using Fr8.Testing.Unit;

namespace terminalDocuSign.Tests.Infrastructure
{
    //public class IndexManagerTests : BaseTest
    //{
    //    IndexManager _indexManager;

    //    [SetUp]
    //    public override void SetUp()
    //    {
    //        base.SetUp();
    //        _indexManager = new IndexManager();          
    //    }

    //    [Test, Ignore("Ignored by yakov.gnusin, does not work, needs further investigation.")]
    //    public void GetEnvelopesTest()
    //    {
    //        //DocuSignEnvelope test = new DocuSignEnvelope();            
    //        //var accountEnvelopes = test.GetEnvelopes(DateTime.Now.AddDays(-1), DateTime.Now, true, 100);
    //        //Assert.IsNotNull(accountEnvelopes);
    //    }       


    //    //This method will test all the methods present in the IndexManger Class
    //    [Test, Ignore("Ignored by yakov.gnusin, does not work, needs further investigation.")]
    //    public void IndexDocuSignUserTest()
    //    {
    //        //AuthorizationTokenDO curAuthorizationTokenDO = new AuthorizationTokenDO()
    //        //{
    //        //    Token = @"{""Email"":""docusign_developer@dockyard.company"",""ApiPassword"":""VIXdYMrnnyfmtMaktD+qnD4sBdU=""}",
    //        //    ExternalAccountId = "docusign_developer@dockyard.company",
    //        //    UserID = "0addea2e-9f27-4902-a308-b9f57d811c0a",

    //        //};
    //        //_indexManager.IndexDocuSignUser(curAuthorizationTokenDO, DateTime.Now.AddDays(-20));
    //    }      
    //}
}