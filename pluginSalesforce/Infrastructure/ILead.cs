using Data.Interfaces.DataTransferObjects;

namespace terminal_Salesforce.Infrastructure
{
    public interface ILead
    {
        bool CreateLead(ActionDTO actionDTO);
    }
}