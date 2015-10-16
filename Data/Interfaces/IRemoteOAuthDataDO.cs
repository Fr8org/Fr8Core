namespace Data.Interfaces
{
    public interface IRemoteOAuthDataDO : IBaseDO
    {
        int Id { get; set; }
        
        int? ProviderID { get; set; }
        IRemoteServiceProviderDO Provider { get; set; }

        string UserID { get; set; }
        IFr8AccountDO User { get; set; }
        
        string Token { get; set; }
        bool HasAccessToken();
    }
}