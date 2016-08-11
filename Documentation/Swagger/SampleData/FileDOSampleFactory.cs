using System;
using Data.Entities;
using Fr8.Infrastructure.Documentation.Swagger;

namespace HubWeb.Documentation.Swagger
{
    public class FileDOSampleFactory : ISwaggerSampleFactory<FileDO>
    {
        public FileDO GetSampleData()
        {
            return new FileDO
            {
                Id = 1,
                LastUpdated = DateTimeOffset.Now,
                CreateDate = DateTimeOffset.Now,
                CloudStorageUrl = "http://yourdomain.com/file_name.jpg",
                DockyardAccountID = "F7C0E406-EB52-40B8-BCE4-A985569272F2",
                OriginalFileName = "FileName.jpg"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}