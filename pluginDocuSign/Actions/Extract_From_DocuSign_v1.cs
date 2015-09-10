using Data.Entities;
using PluginBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;

namespace pluginDocuSign.Actions
{
    public class Extract_From_DocuSign_v1 : BasePluginAction
    {
        public object Configure(ActionDO curActionDO)
        {
            //TODO: The coniguration feature for Docu Sign is not yet defined. The configuration evaluation needs to be implemented.
            return ProcessConfigurationRequest(curActionDO, actionDo => ConfigurationRequestType.Initial); // will be changed to complete the config feature for docu sign
        }

        public object Activate(ActionDO curActionDO)
        {
            return "Activate Request"; // Will be changed when implementation is plumbed in.
        }

        public object Execute(ActionDO curActionDO)
        {
            return "Execute Request"; // Will be changed when implementation is plumbed in.
        }
    }
}