using Utilities.Configuration;

namespace HubTests.Services
{
    public class DisableCacheSettings : IApplicationSettings
    {
        public IApplicationSettings Base;
        
        public DisableCacheSettings(IApplicationSettings @base)
        {
            Base = @base;
        }

        public string GetSetting(string name)
        {
            if (name == "DisableATandTCache")
            {
                return "true";
            }

            return Base != null ? Base.GetSetting(name) : null;
        }
    }
}