using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;

namespace Core.PluginRegistrations
{
    public class FilterUsingRunTimeDataPluginRegistration_v1 : BasePluginRegistration
    {
        public const string PluginRegistrationName = "FilterUsingRunTimeData";
        // Base URL for Web project.
        public const string BaseUrl = "http://localhost:30643/";

        public FilterUsingRunTimeDataPluginRegistration_v1()
            : base(InitAvailableActions(), BaseUrl, PluginRegistrationName)
        {
        }

        private static ActionNameListDTO InitAvailableActions()
        {
            ActionNameListDTO curActionNameList = new ActionNameListDTO();
            ActionNameDTO curActionName = new ActionNameDTO();

            curActionName.Name = "Filter Using Run-Time Data";
            curActionName.Version = "1";
            curActionNameList.ActionNames.Add(curActionName);
            return curActionNameList;
        }
    }
}
