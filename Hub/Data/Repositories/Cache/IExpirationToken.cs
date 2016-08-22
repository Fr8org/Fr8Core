namespace Data.Repositories.Cache
{
    public interface IExpirationToken
    {
        bool IsExpired();
    }
}
