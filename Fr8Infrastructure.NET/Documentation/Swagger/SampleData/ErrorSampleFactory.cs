using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class ErrorSampleFactory : ISwaggerSampleFactory<ErrorDTO>
    {
        public ErrorDTO GetSampleData()
        {
            return ErrorDTO.InternalError("Somehting bad has just happened", "400", "More info on what has happened", string.Empty, "terminalFr8Core");
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}