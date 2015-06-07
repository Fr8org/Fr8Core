using Data.Validations;
using FluentValidation;
using Utilities;

namespace KwasantCore.Services
{
    public class Calendar
    {
        public string ProcessQuickCopy(string copyType, string selectedText)
        {
            string returnvalue = "";
            switch (copyType)
            {
                case "attendees":
                    try
                    {
                        RegexUtilities.ValidateEmailAddress(selectedText);
                        returnvalue = selectedText;
                    }
                    catch(ValidationException ex) { returnvalue = "Invalid Selection"; }
                    break;
                case "start":
                    returnvalue = selectedText.GenerateDateFromText();
                    //returnvalue = "06/25/2014 01:00 pm".GenerateDateFromText();
                    break;
                case "end":
                    returnvalue = selectedText.GenerateDateFromText();
                    //returnvalue = "06/25/2014 01:30 pm";
                    break;
            }
            return returnvalue;
        }
    }
}
