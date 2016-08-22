using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Testing.Unit.Fixtures
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
