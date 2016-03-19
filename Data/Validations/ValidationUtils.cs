using System;
using System.Net.Mail;

namespace Data.Validations
{
    public static class ValidationUtils
    {
        public static bool IsValidEmailAddress(this string email)
        {
            email = email ?? string.Empty;
            try
            {
                var mailAddress = new MailAddress(email.Trim());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
