using System.Data.Entity.Infrastructure;

namespace Data.Interfaces
{
    /// <summary>
    /// Implementing this interface will allow you to perform pre-save and post-save processing.
    /// </summary>
    public interface ISaveHook
    {
        void BeforeSave();
    }

    public interface IModifyHook
    {
        void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues);
    }

    public interface IDeleteHook
    {
        void OnDelete(DbPropertyValues originalValues);
    }

    public interface ICreateHook
    {
        void BeforeCreate();
        void AfterCreate();
    }
}
