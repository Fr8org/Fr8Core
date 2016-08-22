using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class PageDefinitionSampleFactory : ISwaggerSampleFactory<PageDefinitionDTO>
    {
        public PageDefinitionDTO GetSampleData()
        {
            return new PageDefinitionDTO
            {
                Id = 1,
                Type = "Some Type",
                Tags = "this, that",
                Description = "Description goes here",
                Url = "http://yourdomain.com/link_to_your_page",
                PageName = "Name",
                Title = "Very attractive text"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}