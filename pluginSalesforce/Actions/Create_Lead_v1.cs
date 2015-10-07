using Data.Interfaces.DataTransferObjects;
using StructureMap;
using terminal_base.BaseClasses;
using terminal_Salesforce.Infrastructure;

namespace terminal_Salesforce.Actions
{
    public class Create_Lead_v1 : BaseTerminalAction
    {
        ILead _salesforce = ObjectFactory.GetInstance<ILead>();

        public ActionDTO CreateLead(ActionDTO curActionDTO)
        {
            bool result = _salesforce.CreateLead(curActionDTO);
            return curActionDTO;
        }
    }
}