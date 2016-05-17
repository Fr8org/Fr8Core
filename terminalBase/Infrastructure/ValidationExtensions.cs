using Fr8Data.DataTransferObjects;

namespace TerminalBase.Infrastructure
{
    public static class ValidationExtensions
    {
        public static void ValidateEmail(this ValidationManager validationManager, ControlDefinitionDTO control, string errorMessage = null)
        {
            if (!Utilities.RegexUtilities.IsValidEmailAddress(control.Value))
            {
                validationManager.SetError(errorMessage ?? "Not a valid e-mail address", control);
            }
        }
    }
}