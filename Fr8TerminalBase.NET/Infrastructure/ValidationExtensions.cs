using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities;
using PhoneNumbers;

namespace Fr8.TerminalBase.Infrastructure
{
    public static class ValidationExtensions
    {
        public static void ValidateTransitions(this ValidationManager validationManager, ContainerTransition control)
        {
            for (var i = 0; i < control.Transitions.Count; i++)
            {
                var transition = control.Transitions[i];
                if (transition.Transition.RequiresTargetNodeId() && transition.TargetNodeId == null)
                {
                    validationManager.SetError(GetMissingNodeTransitionErrorMessage(transition.Transition), $"transition_{i}");
                }
            }
        }

        private static string GetMissingNodeTransitionErrorMessage(ContainerTransitions transition)
        {
            switch (transition)
            {
                case ContainerTransitions.JumpToActivity:
                    return "Target activity is not specified";
                case ContainerTransitions.LaunchAdditionalPlan:
                    return "Target plan is not specified";
                case ContainerTransitions.JumpToSubplan:
                    return "Target subplan is not specified";
                default:
                    return string.Empty;
            }
        }

        public static void ValidateEmail(this ValidationManager validationManager, IConfigRepository configRepository, ControlDefinitionDTO control, string errorMessage = null)
        {
            if (!RegexUtilities.IsValidEmailAddress(configRepository, control.Value))
            {
                validationManager.SetError(errorMessage ?? "Not a valid e-mail address", control);
            }
        }

        public static void ValidateEmail(this ValidationManager validationManager, IConfigRepository configRepository, TextSource textSource, string errorMessage = null)
        {
            //The validation actually won't go further only if Upstream is set as source but payload is not avaialable. That means we can't yet validate
            if (textSource.HasUpstreamValue)
            {
                return;
            }

            if (!RegexUtilities.IsValidEmailAddress(configRepository, textSource.TextValue))
            {
                validationManager.SetError(errorMessage ?? "Not a valid e-mail address", textSource);
            }
        }

        public static bool ValidatePhoneNumber(this ValidationManager validationManager, string number, TextSource control, string errorMessage = null)
        {
            try
            {
                var phoneUtil = PhoneNumberUtil.GetInstance();
                var isAlphaNumber = phoneUtil.IsAlphaNumber(number);
                var phoneNumber = phoneUtil.Parse(number, "");
                if (isAlphaNumber || !phoneUtil.IsValidNumber(phoneNumber))
                {
                    validationManager.SetError(string.IsNullOrWhiteSpace(errorMessage) ? $"{control.InitialLabel} is invalid" : errorMessage, control);
                    return false;
                }
            }
            catch (NumberParseException npe)
            {
                validationManager.SetError(string.IsNullOrWhiteSpace(errorMessage) ? $"Failed to parse {control.InitialLabel} {npe.Message}" : errorMessage, control);
                return false;
            }
            return true;
        }


        public static bool ValidateTextSourceNotEmpty(this ValidationManager validationManager, TextSource control, string errorMessage)
        {
            ////here is a check for design time
            if (validationManager.Payload == null && control != null && !control.HasValue)
            {
                validationManager.SetError(errorMessage, control);
                return false;
            }

            //this is a check for runtime
            if (control != null && (validationManager.Payload != null) && string.IsNullOrWhiteSpace(control.TextValue))
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

        public static bool ValidateDropDownListNotEmpty(this ValidationManager validationManager, DropDownList control, string errorMessage)
        {
            if (control != null && control.Value.IsNullOrEmpty())
            {
                validationManager.SetError(errorMessage, control);
                return false;
            }

            return true;
        }
    }
}
