namespace Data.Interfaces
{
    public interface IPluginRegistrationDO : IBaseDO
    {
        int Id { get; set; }
        string Name { get; set; }
        int PluginStatus { get; set; }
    }
}