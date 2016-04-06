namespace terminalSalesforce.Infrastructure
{
    public interface ISalesforceObject
    {
        bool Validate();

        string SalesforceObjectType { get; }
    }
}