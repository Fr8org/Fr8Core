using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Routing.Constraints;
using System.Web.Mvc;
using AutoMapper;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using HealthMonitor.Utility;
using Hub.Interfaces;
using Hub.Managers;
using Hub.StructureMap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Infrastructure.AutoMapper;
using terminalDocuSign.Infrastructure.StructureMap;
using terminalDocuSign.Services.New_Api;
using terminalIntegrationTests.Fixtures;
using terminalIntegrationTests.Helpers;
using TerminalBase.Infrastructure;
using UtilitiesTesting;

namespace terminalIntegrationTests
{
    [Explicit]
    [Category("Integration.CustomPlans")]
    public class Query_DocuSign_Into_Google_Sheet_Tests : BaseHubIntegrationTest
    {
        #region Properties

        public override string TerminalName { get; }
        private AuthorizationTokenDTO DocuSignToken;

        #endregion

        [Test]
        public async Task Query_DocuSign_Into_Google_Sheet_End_To_End()
        {
            //
            // SetUp
            //
            var activityConfigurator = new ActivityConfigurator();
            await RevokeTokens();
            
            //create a new envelope that will be put into drafts.
            //var authorizationToken = await DocuSign_AuthToken(string terminalI);
            //var envelopeSummary = CreateNewDocuSignEnvelope(Mapper.Map<AuthorizationTokenDTO, AuthorizationTokenDO>(authorizationToken));

            //create a new plan
            var thePlan = await activityConfigurator.CreateNewPlan();
            
            //configure an query_DocuSign activity
            await activityConfigurator.AddAndConfigure_QueryDocuSign(thePlan, 1);
            
            //login to google
            //configure a save_to google activity
            await activityConfigurator.AddAndConfigure_SaveToGoogleSheet(thePlan,2, "DocuSign Envelope", "DocuSign Envelope Data");

            //create a new empty sheet inside google
            //create an new envelope with statuts draft /use handle template data inside the Send DocuSign Envelope activity

            //run the plan
            await HttpPostAsync<string, string>(_baseUrl + "plans/run?planId=" + thePlan.Plan.Id, null);
            //add asserts here
            //cleanup. erase the sheet
        }

    }
}
