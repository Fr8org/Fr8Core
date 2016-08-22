using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class FactSampleFactory : ISwaggerSampleFactory<FactDTO>
    {
        public FactDTO GetSampleData()
        {
            return new FactDTO
            {
                Id = 1,
                Fr8UserId = "A8FA132D-2181-4B24-A74D-E234951F9477",
                CreateDate = DateTimeOffset.Now,
                Data = "Build_Message activity was executed",
                Status = "Logged",
                Activity = "7F99153E-439F-48DB-A894-B8AFB957143B",
                Component = "Terminal",
                CreatedByID = "3F54489D-CD22-405A-9ABE-B1E711DA0F10",
                ObjectId = "7F99153E-439F-48DB-A894-B8AFB957143B",
                PrimaryCategory = "Execution",
                SecondaryCategory = ""
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}