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
        ILead _salesforce = ObjectFactory.GetInstance<ILead>();

        public ActionDTO CreateLead(ActionDTO curActionDTO)
        {
            bool result=_salesforce.CreateLead(curActionDTO);
            return curActionDTO;
        }
    }
}