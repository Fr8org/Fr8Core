using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;

namespace Utilities
{
    public class RegexUtilities
    {
        public static void ValidateEmailAddress(String emailAddress)
        {
            if (!IsValidEmailAddress(emailAddress))
                throw new ValidationException(new [] { new ValidationFailure("emailAddress", "Invalid email address: '" + emailAddress + "'")});
        }

        public static bool IsValidEmailAddress(String emailAddress)
        {
            return ExtractFromString(emailAddress, true, true).Count == 1;
        }

        public static List<ParsedEmailAddress> ExtractFromString(String textToSearch, bool includeReserved = false, bool strict = false)
        {
            if (String.IsNullOrEmpty(textToSearch))
                return new List<ParsedEmailAddress>();
            //This is the email regex.
            //It searches for emails in the format of <Some Person>somePerson@someDomain.someExtension

            //We assume that names can only contain letters, numbers, and spaces. We also allow for a blank name, in the form of <>
            const string nameRegex = @"[ a-zA-Z0-9]*";

            //We assume for now, that emails can only contain letters, numbers, dashes, dots, +. This can be updated in the future (parsing emails is actually incredibly difficult).
            //See http://tools.ietf.org/html/rfc2822#section-3.4.1 in the future if we ever update this.
            const string emailUserNameRegex = @"[a-zA-Z0-9\!\#\$\%\&\'\*\+\.\-\/\=\?\^_\`\{\|\}\~]+";

            //Domains can only contain letters, numbers, or dashes.
            const string domainRegex = @"[a-zA-Z0-9\-]+";

            //Top level domain must be at least two characters long. Only allows letters, numbers, dashes or dots.
            const string tldRegex = @"[a-zA-Z0-9\-\.]{2,}";

            //The name part is optional; we can find emails like 'rjrudman@gmail.com', or '<Robert Rudman>rjrudman@gmail.com'.
            //The regex uses named groups; 'name' and 'email'.
            //Name will contain the name, without <>. Email will contain the full email address (without the name).

            //Typically, you won't need to modify the below code, only the four variables defined above.
            var fullRegexExpression = String.Format(@"(<(?<name>{0})>)?(?<email>{1}@{2}\.{3})", nameRegex,emailUserNameRegex, domainRegex, tldRegex);

            if (strict)
                fullRegexExpression = String.Format("^{0}$", fullRegexExpression);

            var regex = new Regex(fullRegexExpression);

            var result = new List<ParsedEmailAddress>();
            foreach (Match match in regex.Matches(textToSearch))
            {
                var parse = new ParsedEmailAddress
                {
                    Name = match.Groups["name"].Value,
                    Email = match.Groups["email"].Value.ToLower()
                };

                if (includeReserved || !FilterUtility.IsReservedEmailAddress(parse.Email))
                    result.Add(parse);
            }
            return result;
        }
    }
}