using System;

namespace Utilities
{
    public class ParsedEmailAddress
    {
        public String Name { get; set; }
        public String Email { get; set; }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Name))
                return Email;
            return String.Format("<{0}>{1}", Name, Email); //is that the right order?
        }
    }
}