using System.Threading.Tasks;
using HealthMonitor.Utility;
using NUnit.Framework;
using terminalIntegrationTests.Helpers;

namespace terminalIntegrationTests
{
    [Explicit]
    [Category("Integration.CustomPlans")]
    public class Query_DocuSign_Into_Google_Sheet_Tests : BaseHubIntegrationTest
    {
        #region Properties

        public override string TerminalName { get; }

        #endregion

        [Test]
        public async Task Query_DocuSign_Into_Google_Sheet_End_To_End()
        {
            var activityConfigurator = new ActivityConfigurator(this);
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
            await activityConfigurator.AddAndConfigure_SaveToGoogleSheet(thePlan,2, "Docusign Envelope", "DocuSign Envelope Data");

            //create a new empty sheet inside google
            //create an new envelope with statuts draft /use handle template data inside the Send DocuSign Envelope activity

            //run the plan
            await HttpPostAsync<string, string>(_baseUrl + "plans/run?planId=" + thePlan.Plan.Id, null);
            //add asserts here
            //cleanup. erase the sheet
        }

    }
}
