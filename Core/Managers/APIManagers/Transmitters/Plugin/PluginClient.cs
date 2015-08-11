using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Managers.APIManagers.Packagers.Json;
using Core.Managers.APIManagers.Transmitters.Restful;
using Newtonsoft.Json;

namespace Core.Managers.APIManagers.Transmitters.Plugin
{
    class PluginClient : RestfulServiceClient, IPluginClient
    {
    }
}
