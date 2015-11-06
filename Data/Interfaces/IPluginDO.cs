namespace Data.Interfaces
{
    public interface IPluginDO : IBaseDO
    {
        int Id { get; set; }
        string Name { get; set; }
        int PluginStatus { get; set; }
        string Endpoint { get; set; }

        // TODO: remove this, DO-1397
        // bool RequiresAuthentication { get; set; }
    }
}