using Data.Entities;
using PluginUtilities.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pluginDocuSign.Actions
{
    public class Extract_From_DocuSign_v1 : ActionHandler
    {
        public object HandleConfigureRequest(ActionDO curActionDTO)
        {
            return "Configure Request"; // Will be changed when implementation is plumbed in.
        }

        public object HandleActivateRequest(ActionDO curActionDTO)
        {
            return "Activate Request"; // Will be changed when implementation is plumbed in.
        }

        public object HandleExecuteRequest(ActionDO curActionDTO)
        {
            return "Execute Request"; // Will be changed when implementation is plumbed in.
        } 
    }
}