using Fr8Data.Crates.Helpers;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static EventReportCM TestEmptyEventDto()
        {
            return new EventReportCM { EventNames = string.Empty };
        }

        public static EventReportCM TestTerminalIncidentDto()
        {

            var loggingDataCrate = new LoggingDataCrateFactory().Create(new LoggingDataCM
                {
                    PrimaryCategory = "Operations",
                    SecondaryCategory = "Action"
                });
            var eventDto = new EventReportCM
            {
                EventNames = "Terminal Incident",
                ExternalAccountId = "system1@fr8.co"
            };

            eventDto.EventPayload.Add(loggingDataCrate);

            return eventDto;
        }

        public static EventReportCM TestTerminalEventDto()
        {
            var loggingDataCrate = new LoggingDataCrateFactory().Create(new LoggingDataCM
                {
                    PrimaryCategory = "Operations",
                    SecondaryCategory = "Action"
                });
            var eventDto = new EventReportCM
            {
                EventNames = "Terminal Fact",
                ExternalAccountId = "system1@fr8.co"
            };

            eventDto.EventPayload.Add(loggingDataCrate);

            return eventDto;
        }
    }
}
