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
        //public static DocuSignEnvelope TestEnvelope2(Account account)
        //{
        //    // create envelope object and assign login info
        //    return new DocuSignEnvelope
        //    {
        //        // assign account info from above
        //        Login = account,
        //        // "sent" to send immediately, "created" to save envelope as draft
        //        Status = "created",
        //        Created = DateTime.UtcNow,
        //        Recipients = TestRecipients1()
        //    };
        //}

        public static DocuSignAuthTokenDTO TestDocuSignAuthDTO1()
        {
            return new DocuSignAuthTokenDTO() { Email = "freight.testing@gmail.com", ApiPassword = "SnByDvZJ/fp9Oesd/a9Z84VucjU=" };
        }
    }
}