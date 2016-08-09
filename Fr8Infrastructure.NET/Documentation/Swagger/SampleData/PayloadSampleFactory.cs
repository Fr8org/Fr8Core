using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class PayloadSampleFactory : ISwaggerSampleFactory<PayloadDTO>
    {
        private readonly ISwaggerSampleFactory<CrateStorageDTO> _crateStorageSampleFactory;
        public PayloadSampleFactory(ISwaggerSampleFactory<CrateStorageDTO> crateStorageSampleFactory)
        {
            _crateStorageSampleFactory = crateStorageSampleFactory;
        }

        public PayloadDTO GetSampleData()
        {
            return new PayloadDTO(Guid.Parse("FAEFE292-E010-47B0-A69D-7EDFB4C54050")) { CrateStorage = _crateStorageSampleFactory.GetSampleData() };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}