using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class FieldSampleFactory : ISwaggerSampleFactory<FieldDTO>
    {
        public FieldDTO GetSampleData()
        {
            return new FieldDTO
            {
                Name = "Name",
                Label = "Display Name",
                Tags = "something, goes, here",
                Availability = AvailabilityType.RunTime,
                SourceActivityId = "C94E3A20-1906-43E6-919B-160F7E0EC678",
                FieldType = "int",
                SourceCrateLabel = "Parent Crate",
                IsRequired = true,
                SourceCrateManifest = new CrateManifestType("FieldDescription", (int)MT.FieldDescription)
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}