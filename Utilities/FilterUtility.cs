using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StructureMap;

namespace Utilities
{
    public static class FilterUtility
    {
        private static readonly HashSet<String> _IgnoreEmails;
        static FilterUtility()
        {
            var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            _IgnoreEmails = new HashSet<String>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var reservedEmail in configRepository.Get("EmailAddress_KwasantReservedList", String.Empty).Split(','))
                _IgnoreEmails.Add(reservedEmail);

            _IgnoreEmails.Add(configRepository.Get("EmailAddress_GeneralInfo"));
            _IgnoreEmails.Add(configRepository.Get("INBOUND_EMAIL_USERNAME"));
        }
        
        public static bool IsReservedEmailAddress(String emailAddress)
        {
            return _IgnoreEmails.Contains(emailAddress);
        }

        public static IEnumerable<string> StripReservedEmailAddresses(IEnumerable<string> attendees, IConfigRepository configRepository)
        {
            return attendees.Where(a => !_IgnoreEmails.Contains(a));
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
