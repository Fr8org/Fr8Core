using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using StructureMap;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Web.Controllers
{
    public class ExternalAuthController : Controller
    {
        private readonly IAction _action;

        public ExternalAuthController()
        {
            _action = ObjectFactory.GetInstance<IAction>();
        }

        [HttpGet]
        public async Task<ActionResult> AuthSuccess(
            [Bind(Prefix="dockyard_plugin")] string pluginName,
            [Bind(Prefix = "version")] string pluginVersion)
        {
            if (string.IsNullOrEmpty(pluginName) || string.IsNullOrEmpty(pluginVersion))
            {
                throw new ApplicationException("PluginName or PluginVersion is not specified.");
            }

            var requestQueryString = Request.Url.Query;
            DockyardAccountDO account;
            PluginDO plugin;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var accountId = User.Identity.GetUserId();

                account = uow.UserRepository.GetByKey(accountId);
                if (account == null)
                {
                    throw new ApplicationException("Could not fetch current user.");
                }

                plugin = uow.PluginRepository
                    .FindOne(x => x.Name == pluginName && x.Version == pluginVersion);
                if (plugin == null)
                {
                    throw new ApplicationException("Could not find plugin.");
                }
            }
            
            var externalAuthenticationDTO = new ExternalAuthenticationDTO()
            {
                RequestQueryString = requestQueryString
            };

            await _action.AuthenticateExternal(account, plugin, externalAuthenticationDTO);

            return View();
        }
    }
}