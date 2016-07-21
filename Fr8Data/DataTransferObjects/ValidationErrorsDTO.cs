using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class ValidationErrorsDTO
    {
        [JsonProperty("validationErrors")]
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
