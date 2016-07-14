using System;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates;
using Newtonsoft.Json;

namespace HubWeb.Documentation.Swagger
{
    public class ActivityDescriptionSampleFactory : ISwaggerSampleFactory<ActivityDescriptionDTO>
    {
        private readonly ISwaggerSampleFactory<CrateStorageDTO> _crateStorageSampleFactory;
        public ActivityDescriptionSampleFactory(ISwaggerSampleFactory<CrateStorageDTO> crateStorageSampleFactory)
        {
            _crateStorageSampleFactory = crateStorageSampleFactory;
        }

        public ActivityDescriptionDTO GetSampleData()
        {
            return new ActivityDescriptionDTO
            {
                Id = Guid.Parse("BA7D9D24-E72F-4A4A-8297-1EFA8EA036E5"),
                Name = "Build_Message_v1",
                Version = "1",
                CrateStorage = JsonConvert.SerializeObject(_crateStorageSampleFactory.GetSampleData()),
                ActivityTemplateId = Guid.Parse("526BE60D-9338-435C-BD61-7724FF587571"),
                OriginalId = "54F6F898-4940-489F-B6B6-7249D9E6091D"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}