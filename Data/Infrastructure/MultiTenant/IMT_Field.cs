using Data.Entities;
using Data.Interfaces;

namespace Data.Infrastructure.MultiTenant
{
    public interface IMT_Field
    {
        int? GetFieldColumnOffset(IUnitOfWork uow, string curMtFieldName, int curMtObjectId);

        int GenerateFieldColumnOffset(IUnitOfWork uow, int curMtObjectId);

        void Add(IUnitOfWork uow, Entities.MT_Field curMtField);
    }
}
