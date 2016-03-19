using System;
using HealthMonitor.Utility;
using NUnit.Framework;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using System.Web.Http;
using StructureMap;
using Data.Interfaces;
using System.Linq;
using Data.Entities;
using System.Collections.Generic;

namespace terminalSalesforceTests.Intergration
{
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class GetSalesforceData_Into_SendEmail_EndToEnd_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalSalesforce"; }
        }

        [Test]
        public async Task SomeTest()
        {
            System.Diagnostics.Debugger.Launch();

            //get required activity templates
            var activityTemplates = await HttpGetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(_baseUrl + "plannodes/available");
            var getData = activityTemplates.Single(at => at.Name.Equals("Receivers")).Activities.Single(a => a.Name.Equals("Get_Data"));
            var sendEmail = activityTemplates.Single(at => at.Name.Equals("Forwarders")).Activities.Single(a => a.Name.Equals("SendEmailViaSendGrid"));
            Assert.IsNotNull(getData, "Get Salesforce Data activity is not available");
            Assert.IsNotNull(sendEmail, "Send Email activity is not available");

            //get salesforce auth token
            AuthorizationTokenDO salesforceAuth;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                salesforceAuth = uow.AuthorizationTokenRepository.GetPublicDataQuery().Single(t => t.AdditionalAttributes.StartsWith("refresh_token="));
            }
            Assert.IsNotNull(salesforceAuth, "Salesforce Auth is not available in the database");

            //create initial plan
            var initialPlan = await HttpPostAsync<PlanEmptyDTO, PlanDTO>(_baseUrl + "plans", new PlanEmptyDTO()
            {
                Name = "GetSalesforceData_Into_SendEmail_EndToEnd_Test"
            });

            string mainUrl = _baseUrl + "activities/create";
            var postUrl = "?actionTemplateId={0}&createPlan=false";
            var formattedPostUrl = string.Format(postUrl, getData.Id);
            formattedPostUrl += "&parentNodeId=" + initialPlan.Plan.StartingSubPlanId;
            formattedPostUrl += "&authorizationTokenId=" + salesforceAuth.Id;
            formattedPostUrl += "&order=" + 1;
            formattedPostUrl = mainUrl + formattedPostUrl;
            var getDataActivity = await HttpPostAsync<ActivityDTO>(formattedPostUrl, null);

            formattedPostUrl = string.Format(postUrl, sendEmail.Id);
            formattedPostUrl += "&parentNodeId=" + initialPlan.Plan.StartingSubPlanId;
            formattedPostUrl += "&order=" + 2;
            formattedPostUrl = mainUrl + formattedPostUrl;
            var sendEmailActivity = await HttpPostAsync<ActivityDTO>(formattedPostUrl, null);
        }
    }
}
