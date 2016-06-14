using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;

namespace Fr8.Testing.Unit.Fixtures
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
