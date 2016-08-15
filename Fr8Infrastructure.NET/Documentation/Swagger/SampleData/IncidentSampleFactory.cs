using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class IncidentSampleFactory : ISwaggerSampleFactory<IncidentDTO>
    {
        public IncidentDTO GetSampleData()
        {
            return new IncidentDTO
            {
                Id = 1,
                Fr8UserId = "2F8CD0AD-F41F-4119-910A-985F31ADD848",
                CreateDate = DateTimeOffset.Now,
                Data = "Unhandled exception of specific type occured during activity configuration",
                Status = "Logged",
                SecondaryCategory = "ApplicationException",
                ObjectId = "7967113D-89C6-42DE-B702-C0D2216D5DB8",
                Activity = "7967113D-89C6-42DE-B702-C0D2216D5DB8",
                PrimaryCategory = "Configuration",
                Component = "Terminal",
                Priority = 5,
                isHighPriority = true
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}