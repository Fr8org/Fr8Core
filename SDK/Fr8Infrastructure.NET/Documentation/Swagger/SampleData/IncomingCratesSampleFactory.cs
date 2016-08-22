using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class IncomingCratesSampleFactory : ISwaggerSampleFactory<IncomingCratesDTO>
    {
        private readonly ISwaggerSampleFactory<FieldDTO> _fieldSampleFactory;
        private readonly ISwaggerSampleFactory<CrateDescriptionDTO> _crateDescriptionSampleFactory;

        public IncomingCratesSampleFactory(ISwaggerSampleFactory<FieldDTO> fieldSampleFactory, ISwaggerSampleFactory<CrateDescriptionDTO> crateDescriptionSampleFactory)
        {
            _fieldSampleFactory = fieldSampleFactory;
            _crateDescriptionSampleFactory = crateDescriptionSampleFactory;
        }

        public IncomingCratesDTO GetSampleData()
        {
            var result = new IncomingCratesDTO();
            result.AvailableCrates.Add(_crateDescriptionSampleFactory.GetSampleData());
            return result;
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}