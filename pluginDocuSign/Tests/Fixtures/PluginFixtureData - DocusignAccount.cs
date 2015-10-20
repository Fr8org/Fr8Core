using DocuSign.Integrations.Client;
using pluginDocuSign.DataTransferObjects;
using pluginDocuSign.Infrastructure;
using pluginDocuSign.Services;
using System;

namespace pluginDocuSign.Tests.Fixtures
{
    public partial class PluginFixtureData
    {
        public static DocuSignAccount TestDocuSignAccount1()
        {
            // credentials for sending account
            return new DocuSignAccount
                   {
                       Email = "DocuSignTestAccount1@Dockyard.company",
                       Password = "peach23",
                       UserId = " DocuSignTestAccount1@Dockyard.company",
                       UserName = " DocuSignTestAccount1@Dockyard.company"
                   };
        }
        public static DocuSignEnvelope TestEnvelope2(Account account)
        {
            // create envelope object and assign login info
            return new DocuSignEnvelope
            {
                // assign account info from above
                Login = account,
                // "sent" to send immediately, "created" to save envelope as draft
                Status = "created",
                Created = DateTime.UtcNow,
                Recipients = TestRecipients1()
            };
        }

        public static DocuSignAuthDTO TestDocuSignAuthDTO1()
        {
            return new DocuSignAuthDTO() { Email = "64684b41-bdfd-4121-8f81-c825a6a03582", ApiPassword = "H1e0D79tpJ3a/7klfhPkPxNMcOo=" };
        }
    }
}