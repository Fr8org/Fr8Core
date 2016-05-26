using Fr8Data.Control;
using Fr8Data.DataTransferObjects;
using PhoneNumbers;
using Utilities;

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

        public static void ValidateEmail(this ValidationManager validationManager, TextSource textSource, string errorMessage = null)
        {
            //The validation actually won't go further only if Upstream is set as source but payload is not avaialable. That means we can't yet validate
            if (!textSource.CanGetValue(validationManager.Payload) && !textSource.ValueSourceIsNotSet)
            {
                return;
            }
            var value = textSource.CanGetValue(validationManager.Payload) ? textSource.GetValue(validationManager.Payload) : string.Empty;
            if (!RegexUtilities.IsValidEmailAddress(value))
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
                    validationManager.SetError(control.InitialLabel + " Is Invalid", control);
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

        public static bool ValidateCrateChooserNotEmpty(this ValidationManager validationManager, CrateChooser crateChooser, string errorMessage)
        {
            if (!crateChooser.HasValue)
            {
                validationManager.SetError(errorMessage, crateChooser);
                return false;
            }

            if (crateChooser.CanGetValue(validationManager.Payload) && crateChooser.GetValue(validationManager.Payload) == null)
            {
                validationManager.SetError(errorMessage, crateChooser);
                return false;
            }

            return true;
        }
    }
}
