namespace Data.Interfaces
{
    public interface IPluginDO : IBaseDO
    {
        int Id { get; set; }
        string Name { get; set; }
        string Version { get; set; }
        int PluginStatus { get; set; }
        string Endpoint { get; set; }
    }
}