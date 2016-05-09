
using Fr8Data.DataTransferObjects;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ActivityNameDTO TestActivityNameDTO1()
        {
            return new ActivityNameDTO
            {
                Name = "Write SQL",
                Version = "1.0"
            };
        }
    }
}
