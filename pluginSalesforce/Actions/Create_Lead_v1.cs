using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PluginBase.BaseClasses;
using Data.Interfaces.DataTransferObjects;
using Core.Interfaces;
using StructureMap;
using pluginSalesforce.Infrastructure;

namespace pluginSalesforce.Actions
{
    public class Create_Lead_v1 : BasePluginAction
    {
        ISalesforce _salesforce = ObjectFactory.GetInstance<ISalesforce>();

        public ActionDTO CreateLead(ActionDTO curActionDTO)
        {
            bool result=_salesforce.CreateLead(curActionDTO);

            return curActionDTO;
        }
    }
}