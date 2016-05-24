using Data.Validations;
using Fr8Data.Control;
using Fr8Data.DataTransferObjects;
using PhoneNumbers;

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

        public static void ValidateEmail(this ValidationManager validationManager, TextSource textSource, string errorMessage = null)
        {
            var value = textSource.CanGetValue(validationManager.Payload) ? textSource.GetValue(validationManager.Payload) : string.Empty;
            if (!value.IsValidEmailAddress())
            {
                validationManager.SetError(errorMessage ?? "Not a valid e-mail address", textSource);
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


    }
}
