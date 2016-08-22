using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class CrateDescriptionSampleFactory : ISwaggerSampleFactory<CrateDescriptionDTO>
    {
        private readonly ISwaggerSampleFactory<FieldDTO> _fieldSampleFactory;
        public CrateDescriptionSampleFactory(ISwaggerSampleFactory<FieldDTO> fieldSampleFactory)
        {
            _fieldSampleFactory = fieldSampleFactory;
        }

        public CrateDescriptionDTO GetSampleData()
        {
            return new CrateDescriptionDTO
            {
                Label = "Crate Label",
                Availability = AvailabilityType.RunTime,
                ManifestId = (int)MT.FieldDescription,
                ManifestType = "FieldDescriptionCM",
                ProducedBy = "Fr8",
                Fields = new List<FieldDTO> { _fieldSampleFactory.GetSampleData() },
                Selected = false
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}