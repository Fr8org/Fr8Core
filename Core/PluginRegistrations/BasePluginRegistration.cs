using System.Collections.Generic;

namespace Core.PluginRegistrations
{
    public class BasePluginRegistration : IPluginRegistration
    {
        private string _baseUrl = string.Empty;
        private IEnumerable<string> _availableCommands = null;
        public BasePluginRegistration(string baseUrl, IEnumerable<string> availableCommands)
        {
            this._baseUrl = BaseUrl;
            this._availableCommands = availableCommands;
        }

        public string BaseUrl { get { return _baseUrl; } }

        public IEnumerable<string> AvailableCommands { get { return _availableCommands; } }
    }
}