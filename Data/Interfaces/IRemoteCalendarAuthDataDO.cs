namespace Data.Interfaces
{
    public interface IRemoteCalendarAuthDataDO : IBaseDO
    {
        int Id { get; set; }
        
        int? ProviderID { get; set; }
        IRemoteCalendarProviderDO Provider { get; set; }

        string UserID { get; set; }
        IUserDO User { get; set; }
        
        string AuthData { get; set; }
        bool HasAccessToken();
    }
}