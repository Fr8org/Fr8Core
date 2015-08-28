using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static ExternalEventSubscriptionDO TestExternalEventSubscription_medical_form_v1()
        {
            return new ExternalEventSubscriptionDO
            {
                       Id = 455,
                       ExternalEvent = ExternalEventType.EnvelopeSent
            };
        }
    }
}