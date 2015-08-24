using System;
using Data.Entities;
using Data.States;
using Data.Wrappers;
using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static DocuSignEventDO TestDocuSignEvent1()
        {
            return new DocuSignEventDO
            {
                   EnvelopeId    = "36",
                   ExternalEventType = ExternalEventType.EnvelopeSent
            };
        }
    }
}