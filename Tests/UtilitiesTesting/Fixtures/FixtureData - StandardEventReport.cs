using Data.Crates;
using Data.Interfaces.Manifests;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static Crate RawStandardEventReportFormat()
        {
            var eventReportMS = new EventReportCM()
            {
                EventNames = "DocuSign Envelope Sent"
            };

            return Crate.FromContent("Standard Event Report", eventReportMS);
        }

        public static EventReportCM StandardEventReportFormat()
        {
            return new EventReportCM
            {
                EventNames = "DocuSign Envelope Sent"
            };
        }
    }
}
