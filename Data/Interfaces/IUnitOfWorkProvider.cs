namespace Data.Interfaces
{
    public interface IUnitOfWorkProvider
    {
        IUnitOfWork GetNewUnitOfWork();
    }
}
