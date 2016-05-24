using Fr8Data.Control;
using Fr8Data.DataTransferObjects;
using PhoneNumbers;

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

        public static bool ValidatePhoneNumber(this ValidationManager validationManager, string number, TextSource control)
        {
            try
            {
                PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

                bool isAlphaNumber = phoneUtil.IsAlphaNumber(number);
                PhoneNumber phoneNumber = phoneUtil.Parse(number, "");
                if (isAlphaNumber || !phoneUtil.IsValidNumber(phoneNumber))
                {
                    validationManager.SetError( control.InitialLabel + " Is Invalid", control);
                    return false;
                }

            }
            catch (NumberParseException npe)
            {
                validationManager.SetError("Failed to parse " + control.InitialLabel + " " + npe.Message, control);
                return false;
            }

            return true;
        }


        public static bool ValidateTextSourceNotEmpty(this ValidationManager validationManager, TextSource control, string errorMessage)
        {
            if (control != null && control.CanGetValue(validationManager.Payload) && string.IsNullOrWhiteSpace(control.GetValue(validationManager.Payload)))
            {
                validationManager.SetError(errorMessage, control);
                return false;
            }

            return true;
        }
    }
}
