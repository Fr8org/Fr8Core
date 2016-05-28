using System;
using System.Collections.Generic;
using System.Linq;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;

namespace TerminalBase.Infrastructure
{
    public class ValidationManager
    {
        private readonly ValidationResultsCM _validationResults;

        public bool HasErrors => _validationResults?.ValidationErrors?.Count > 0;
        public ValidationResultsCM ValidationResults => _validationResults;
        public ICrateStorage Payload { get; }

        public ValidationManager(ICrateStorage payload)
        {
            _validationResults = new ValidationResultsCM();
            Payload = payload;
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
            SetError(errorMessage, controls.Select(ResolveControlName).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
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

        protected virtual string ResolveControlName(ControlDefinitionDTO control)
        {
            return control?.Name;
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
