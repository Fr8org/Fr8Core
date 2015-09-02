using System.Collections.Generic;
using Data.Entities;

namespace Core.Services
{
    public interface IPlugin
    {
        IEnumerable<PluginDO> GetAll();
    }
}