using Data.Entities;
using Data.Interfaces;
using System.Collections.Generic;
using System.Linq;
namespace Data.Repositories
{
    public class MTFieldTypeRepository : GenericRepository<MT_FieldType>, IMTFieldTypeRepository
    {
        internal MTFieldTypeRepository(IUnitOfWork uow) : base(uow)
        { }

        public new IEnumerable<MT_FieldType> GetAll()
        {
            var result = new List<MT_FieldType>(DBSet.AsEnumerable());
            result.AddRange(DBSet.Local);
            return result;
        }
    }

}
