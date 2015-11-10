﻿using DocuSign.Integrations.Client;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using System;

namespace terminalDocuSign.Tests.Fixtures
{
    public partial class TerminalFixtureData
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
            return new DocuSignAuthDTO() { Email = "64684b41-bdfd-4121-8f81-c825a6a03582", ApiPassword = "HyCXOBeGl/Ted9zcMqd7YEKoN0Q=" };
        }
    }
}