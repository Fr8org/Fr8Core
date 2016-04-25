namespace Data.Repositories.SqlBased
{
    public interface ISqlConnectionProvider
    {
        object ConnectionInfo { get; }
    }
}
