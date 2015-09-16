using System.Collections.Generic;
using Data.Entities;

namespace Core.Services
{
    public interface IPlugin
    {
        IEnumerable<PluginDO> GetAll();
        string Authorize();

        /// <summary>
        /// Gets the required plugin service URL by Plugin Name and its version
        /// </summary>
        /// <param name="curPluginName">Name of the required plugin</param>
        /// <param name="curPluginVersion">Version of the required plugin</param>
        /// <returns>End Point URL of the required plugin</returns>
        string GetPluginUrl(string curPluginName, string curPluginVersion);
    }
}