using System;
using System.Collections.Generic;
using System.Linq;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;

namespace TerminalBase.Infrastructure
{
    public class ValidationManager
    {
        private readonly ValidationResultsCM _validationResults;

        public bool HasErrors => _validationResults?.ValidationErrors?.Count > 0;

        public ValidationManager(ValidationResultsCM validationResults)
        {
            _validationResults = validationResults;
        }

        /// <summary>
        /// Clears all validation errors
        /// </summary>
        public void Reset()
        {
            CheckSettings();

            _validationResults.ValidationErrors?.Clear();
        }

        public void SetError(string errorMessage, params ControlDefinitionDTO[] controls)
        {
            SetError(errorMessage, controls.Select(x => x.Name).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
        }

        public void SetError(string errorMessage)
        {
            SetError(errorMessage, (string[])null);
        }

        public void SetError(string errorMessage, params string[] controlNames)
        {
            CheckSettings();

            if (_validationResults.ValidationErrors == null)
            {
                _validationResults.ValidationErrors = new List<ValidationResultDTO>();
            }

            _validationResults.ValidationErrors.Add(new ValidationResultDTO
            {
                ErrorMessage = errorMessage,
                ControlNames = controlNames != null ? new List<string>(controlNames) : null
            });
        }

        private void CheckSettings()
        {
            if (_validationResults == null)
            {
                throw new InvalidOperationException("ValidationResultsCM was not set.");
            }
        }
    }
}
