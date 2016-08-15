using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class Fr8DataSampleFactory : ISwaggerSampleFactory<Fr8DataDTO>
    {
        private readonly ISwaggerSampleFactory<ActivityDTO> _activitySampleFactory;
        public Fr8DataSampleFactory(ISwaggerSampleFactory<ActivityDTO> activitySampleFactory)
        {
            _activitySampleFactory = activitySampleFactory;
        }

        public Fr8DataDTO GetSampleData()
        {
            return new Fr8DataDTO
            {
                ActivityDTO = _activitySampleFactory.GetSampleData(),
                ContainerId = Guid.Parse("ED28A098-83D6-421D-854B-9CCB51D11BB5"),
                ExplicitData = string.Empty
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}
