using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fr8.Infrastructure.Utilities
{
    public static class FilterUtility
    {
        public static bool IsReservedEmailAddress(IConfigRepository configRepository, String emailAddress)
        {
            var ignoreEmails = new HashSet<String>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var reservedEmail in configRepository.Get("EmailAddress_KwasantReservedList", String.Empty).Split(','))
            {
                ignoreEmails.Add(reservedEmail);
            }

            ignoreEmails.Add(configRepository.Get("EmailAddress_GeneralInfo"));
            ignoreEmails.Add(configRepository.Get("INBOUND_EMAIL_USERNAME"));

            return ignoreEmails.Contains(emailAddress);
        }
        
        public static string GetState(Type containingType, int value)
        {
            foreach (FieldInfo field in containingType.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if ((int)field.GetValue(field) == value)
                {
                    return field.Name;
                }
            }

            return "";
        }
    }
}
