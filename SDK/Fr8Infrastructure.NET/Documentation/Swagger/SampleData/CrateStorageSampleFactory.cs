using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class CrateStorageSampleFactory : ISwaggerSampleFactory<CrateStorageDTO>
    {
        private readonly ISwaggerSampleFactory<CrateDTO> _crateSampleFactory;
        public CrateStorageSampleFactory(ISwaggerSampleFactory<CrateDTO> crateSampleFactory)
        {
            _crateSampleFactory = crateSampleFactory;
        }

        public CrateStorageDTO GetSampleData()
        {
            return new CrateStorageDTO
            {
                Crates = new[] {_crateSampleFactory.GetSampleData()}
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}