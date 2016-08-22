using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class OrganizationSampleFactory : ISwaggerSampleFactory<OrganizationDTO>
    {
        public OrganizationDTO GetSampleData()
        {
            return new OrganizationDTO
            {
                Id = 1,
                Name = "Foo & Bar",
                BackgroundColor = "blue",
                LogoUrl = "http://fooandbar.foo/logo.png",
                ThemeName = "Black and White"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}