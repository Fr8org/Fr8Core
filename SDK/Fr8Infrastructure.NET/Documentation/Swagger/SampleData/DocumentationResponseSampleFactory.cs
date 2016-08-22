using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class DocumentationResponseSampleFactory : ISwaggerSampleFactory<DocumentationResponseDTO>
    {
        public DocumentationResponseDTO GetSampleData()
        {
            return new DocumentationResponseDTO
            {
                Name = "Header",
                Body = "<p>Some documenations</p>",
                Terminal = "terminalFr8Core",
                Version = 1.0
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}