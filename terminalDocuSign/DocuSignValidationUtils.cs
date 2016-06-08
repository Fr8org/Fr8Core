using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Infrastructure;

namespace terminalDocuSign
{
    public static class DocuSignValidationUtils
    {
        public const string ControlsAreNotConfiguredErrorMessage = "Controls are not configured properly";

        public const string NoTemplateExistsErrorMessage = "Please link at least one template to your DocuSign account";

        public const string TemplateIsNotSelectedErrorMessage = "Template is not selected";

        public const string RecipientIsNotSpecifiedErrorMessage = "Recipient is not specified";

        public const string RecipientIsNotValidErrorMessage = "Specified value is not a valid email address";

        public const string DocumentIsNotValidErrorMessage = "New document is not selected";

        public static bool AtLeastOneItemExists(DropDownList items)
        {
            return items != null && items.ListItems.Count > 0;
        }

        public static bool ItemIsSelected(DropDownList items)
        {
            return items != null && !string.IsNullOrEmpty(items.selectedKey);
        }

        public static bool ValueIsSet(TextBox text)
        {
            return !string.IsNullOrWhiteSpace(text?.Value);
        }

        public static bool ValidateControlExistance(this ValidationManager validationManager, ControlDefinitionDTO control)
        {
            if (control == null)
            {
                validationManager.SetError(ControlsAreNotConfiguredErrorMessage);
                return false;
            }

            return true;
        }

        public static void ValidateTemplateList(this ValidationManager validationManager, DropDownList items)
        {
            if (AtLeastOneItemExists(items))
            {
                if (!ItemIsSelected(items))
                {
                    validationManager.SetError(TemplateIsNotSelectedErrorMessage, items);
                }
            }
            else
            {
                validationManager.SetError(NoTemplateExistsErrorMessage, items);
            }
        }
    }
}