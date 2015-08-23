using System;
using Data.Entities;
using Data.Wrappers;
using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static DocuSignTemplateSubscriptionDO TestDocuSignTemplateSubscription_medical_form_v2()
        {
            return new DocuSignTemplateSubscriptionDO
            {
                       Id = 455,
                       DocuSignTemplateId = " 58521204-58AF-4E65-8A77-4F4B51FEF626",
            };
        }
    }
}