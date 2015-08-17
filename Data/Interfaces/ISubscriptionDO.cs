namespace Data.Interfaces
{
    public interface ISubscriptionDO : IBaseDO
    {
        int Id { get; set; }
        string DockyardAccountId { get; set; }
        IPluginDO PluginRegistration { get; set; }
        int AccessLevel { get; set; }
    }
}