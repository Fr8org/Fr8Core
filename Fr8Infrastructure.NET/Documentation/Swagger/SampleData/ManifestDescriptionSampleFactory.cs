using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class ManifestDescriptionSampleFactory : ISwaggerSampleFactory<ManifestDescriptionDTO>
    {
        public ManifestDescriptionDTO GetSampleData()
        {
            return new ManifestDescriptionDTO
            {
                Id = "1",
                Name = "CustomManifestCM",
                Version = "1",
                Description = "This is a custom manifest",
                RegisteredBy = "Fr8",
                SampleJSON = JsonConvert.SerializeObject(new { dd = 1, name = "name" })
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}