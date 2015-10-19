using Data.Interfaces.DataTransferObjects;
using StructureMap;
using pluginSalesforce.Infrastructure;
using PluginBase.BaseClasses;

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