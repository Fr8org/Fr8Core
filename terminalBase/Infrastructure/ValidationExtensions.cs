using Data.Interfaces.DataTransferObjects;
using Data.Validations;

namespace TerminalBase.Infrastructure
{
    public static class ValidationExtensions
    {
        public static void ValidateEmail(this ValidationManager validationManager, ControlDefinitionDTO control, string errorMessage = null)
        {
            if (!control.Value.IsValidEmailAddress())
            {
                validationManager.SetError(errorMessage ?? "Not a valid e-mail address", control);
            }
        }
    }
}
