using System.Collections.Generic;
using Data.Entities;

namespace Core.Services
{
    public interface IPlugin
    {
        IEnumerable<PluginDO> GetAll();

        /// <summary>
        /// Parses the required plugin service URL for the given action by Plugin Name and its version
        /// </summary>
        /// <param name="curPluginName">Name of the required plugin</param>
        /// <param name="curPluginVersion">Version of the required plugin</param>
        /// <param name="curActionName">Required action</param>
        /// <returns>Parsed URl to the plugin for its action</returns>
        string ParsePluginUrlFor(string curPluginName, string curPluginVersion, string curActionName);
    }
}