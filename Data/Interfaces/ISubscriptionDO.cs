namespace Data.Interfaces
{
    public interface ISubscriptionDO : IBaseDO
    {
        int Id { get; set; }
        string AccountId { get; set; }
        IPluginRegistrationDO PluginRegistration { get; set; }
        int AccessLevel { get; set; }
    }
}