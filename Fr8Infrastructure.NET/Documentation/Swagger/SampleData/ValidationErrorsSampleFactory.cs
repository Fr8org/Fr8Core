using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class ValidationErrorsSampleFactory : ISwaggerSampleFactory<ValidationErrorsDTO>
    {
        private readonly ISwaggerSampleFactory<ValidationResultDTO> _validationResultSampleFactory;
        public ValidationErrorsSampleFactory(ISwaggerSampleFactory<ValidationResultDTO> validationResultSampleFactory)
        {
            _validationResultSampleFactory = validationResultSampleFactory;
        }

        public ValidationErrorsDTO GetSampleData()
        {
            return new ValidationErrorsDTO(new[] {_validationResultSampleFactory.GetSampleData()});
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}