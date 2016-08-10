using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class FileSampleFactory : ISwaggerSampleFactory<FileDTO>
    {
        public FileDTO GetSampleData()
        {
            return new FileDTO
            {
                Id = 1,
                Tags = "{ \"type\" : \"jpg\" }",
                CloudStorageUrl = "http://yourdomain.com/file_name.jpg",
                OriginalFileName = "FileName.jpg"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}