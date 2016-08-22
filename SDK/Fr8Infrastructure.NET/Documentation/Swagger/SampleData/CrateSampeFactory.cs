using System;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Newtonsoft.Json.Linq;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class CrateSampeFactory : ISwaggerSampleFactory<CrateDTO>
    {
        public CrateDTO GetSampleData()
        {
            return new CrateDTO
            {
                Id = "1D6D0F68-82A3-455A-8804-2CEAF4E77523",
                Label = "Some Content",
                Contents = JToken.FromObject(new CrateDescriptionCM(new CrateDescriptionDTO
                {
                    Label = "Some Other Content",
                    Availability = AvailabilityType.RunTime,
                    ManifestId = 30,
                    ManifestType = "CrateDescriptionCM",
                    ProducedBy = "Fr8",
                })),
                ManifestId = 30,
                ManifestType = "CreateDescriptionCM",
                CreateTime = DateTime.Now,
                ParentCrateId = string.Empty,
                SourceActivityId = "BAF3BCB4-3968-4DFF-983B-736E3C0CB24F"
            };

        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}