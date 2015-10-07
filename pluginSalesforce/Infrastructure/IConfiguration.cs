using Salesforce.Force;

namespace terminal_Salesforce.Infrastructure
{
    public interface IConfiguration
    {
        ForceClient GetForceClient();
    }
}