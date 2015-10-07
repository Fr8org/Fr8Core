using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminal_base.BaseClasses;
using Data.Interfaces.DataTransferObjects;
using Core.Interfaces;
using StructureMap;
using pluginSalesforce.Infrastructure;

namespace pluginSalesforce.Actions
{
    public class Create_Lead_v1 : BaseTerminalAction
    {
        ILead _salesforce = ObjectFactory.GetInstance<ILead>();

        public ActionDTO CreateLead(ActionDTO curActionDTO)
        {
            bool result=_salesforce.CreateLead(curActionDTO);
            return curActionDTO;
        }
    }
}