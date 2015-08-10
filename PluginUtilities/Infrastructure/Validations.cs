using System;
using Newtonsoft.Json;

namespace PluginUtilities.Infrastructure
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
    }
}