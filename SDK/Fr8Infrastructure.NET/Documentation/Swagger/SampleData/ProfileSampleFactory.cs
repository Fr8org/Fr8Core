using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class ProfileSampleFactory : ISwaggerSampleFactory<ProfileDTO>
    {
        public ProfileDTO GetSampleData()
        {
            return new ProfileDTO
            {
                Id = "20EF5C0E-3683-4D62-A71B-80708B2B8903",
                Name = "Standard Users"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}