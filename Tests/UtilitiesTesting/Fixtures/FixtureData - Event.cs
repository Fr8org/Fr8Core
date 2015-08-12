using Data.Interfaces.DataTransferObjects;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static EventDTO TestEmptyEventDto()
        {
            return new EventDTO {EventType = string.Empty};
        }

        public static EventDTO TestPluginIncidentDto()
        {
            var eventDto = new EventDTO
            {
                EventType = "Plugin Incident",
                Data = new EventData { PrimaryCategory = "PrimaryCategory", SecondaryCategory = "SecondaryCategory" }
            };
            return eventDto;
        }
    }
}
