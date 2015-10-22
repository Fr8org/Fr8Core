using System;
using Newtonsoft.Json;

namespace TerminalBase.Infrastructure
{
    public static class Validations
    {
        public static bool ValidateDtoString<T>(string dtoString)
        {
            try
            {
                JsonConvert.DeserializeObject<T>(dtoString);
            }
            catch (JsonReaderException e)
            {
                throw new ArgumentException(e.Message);
            }
            return true;
        }

        public static bool ValidateMax1(string[] curList)
        {
            var objCount = curList.Length;
            if (objCount > 1)
                throw new Exception("The list contains more than one items.");
            
            return true;
        }
    }
}