using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class ValidationResultSampleFactory : ISwaggerSampleFactory<ValidationResultDTO>
    {
        public ValidationResultDTO GetSampleData()
        {
            return new ValidationResultDTO
            {
                ControlNames = new List<string> { "Control1", "Control2" },
                ErrorMessage = "Value is not specified"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}