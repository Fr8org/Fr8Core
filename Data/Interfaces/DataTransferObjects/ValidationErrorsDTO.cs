using System.Collections.Generic;

namespace Data.Interfaces.DataTransferObjects
{
    public class ValidationErrorsDTO
    {
        public readonly List<ValidationResultDTO> ValidationErrors = new List<ValidationResultDTO>();

        public ValidationErrorsDTO()
        {
        }


        public ValidationErrorsDTO(IEnumerable<ValidationResultDTO> errors)
        {
            ValidationErrors.AddRange(errors);
        }
    }
}
