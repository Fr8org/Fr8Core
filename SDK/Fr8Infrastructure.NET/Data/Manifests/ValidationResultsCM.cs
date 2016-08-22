using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class ValidationResultsCM : Manifest
    {
        // List of validation errors
        [JsonProperty("validationErrors")]
        public List<ValidationResultDTO> ValidationErrors { get; set; } = new List<ValidationResultDTO>();

        [JsonIgnore]
        public bool HasErrors => ValidationErrors?.Count > 0;

        public ValidationResultsCM() 
            : base(MT.ValidationResults)
        {
        }

        public List<string> GetErrorsForControl(string controlName)
        {
            List<string> errors = new List<string>();

            if (ValidationErrors != null)
            {
                foreach (var validationResultDto in ValidationErrors)
                {
                    if (validationResultDto.ControlNames != null && !string.IsNullOrWhiteSpace(validationResultDto.ErrorMessage))
                    {
                        if (validationResultDto.ControlNames.Any(x => x == controlName))
                        {
                            errors.Add(validationResultDto.ErrorMessage);
                        }
                    }
                }
            }

            return errors;
        }

        public override string ToString()
        {
            if (ValidationErrors != null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine($"{ValidationErrors.Count} validation error(s).");

                foreach (var validationResult in ValidationErrors)
                {
                    if (validationResult.ControlNames?.Count > 0)
                    {
                        stringBuilder.Append("Controls [");
                        bool first = true;

                        foreach (var controlName in validationResult.ControlNames)
                        {
                            if (!first)
                            {
                                stringBuilder.Append(", ");
                            }

                            stringBuilder.Append(controlName);

                            first = false;
                        }

                        stringBuilder.Append("]: ");
                    }
                    else
                    {
                        stringBuilder.Append("Global: ");
                    }

                    stringBuilder.AppendLine(validationResult.ErrorMessage);
                }

                return stringBuilder.ToString();
            }

            return "No validation errors";
        }
    }
}
