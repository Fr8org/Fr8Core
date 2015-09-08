
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace Core.Services
{
    /// <summary>
    /// File service
    /// </summary>
    public class Plugin : IPlugin
    { 
        public IEnumerable<PluginDO> GetAll()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.PluginRepository.GetAll();
            }
        }

        public string Authorize()
        {
            return "AuthorizationToken";
        }
    }
}
