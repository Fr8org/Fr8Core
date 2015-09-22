using System.Collections.Generic;
using Data.Crates.Helpers;
using Data.Interfaces.DataTransferObjects;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static EventDTO TestEmptyEventDto()
        {
            return new EventDTO { EventName = string.Empty };
        }

        public static EventDTO TestPluginIncidentDto()
        {

            var loggingDataCrate = new LoggingDataCrateFactory().Create(new LoggingData
                {
                    PrimaryCategory = "Operations",
                    SecondaryCategory = "Action"
                });
            var eventDto = new EventDTO
            {
                EventName = "Plugin Incident",
                CrateStorage = new List<CrateDTO> { loggingDataCrate }
            };
            return eventDto;
        }

        public static EventDTO TestPluginEventDto()
        {
            var loggingDataCrate = new LoggingDataCrateFactory().Create(new LoggingData
                {
                    PrimaryCategory = "Operations",
                    SecondaryCategory = "Action"
                });
            var eventDto = new EventDTO
            {
                EventName = "Plugin Event",
                CrateStorage = new List<CrateDTO> { loggingDataCrate }
            };
            return eventDto;
        }
    }
}
