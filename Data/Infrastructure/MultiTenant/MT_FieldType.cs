using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Infrastructure.MultiTenant
{
    public class MT_FieldType : IMT_FieldType
    {
        public Data.Entities.MT_FieldType GetOrCreateMT_FieldType(IUnitOfWork _uow, Type type)
        {
            var correspondingMTFieldType = _uow.MTFieldTypeRepository.GetAll().Where(a => a.TypeName == type.FullName).FirstOrDefault();
            if (correspondingMTFieldType == null)
            {
                correspondingMTFieldType = new Data.Entities.MT_FieldType();
                correspondingMTFieldType.AssemblyName = type.Assembly.FullName;
                correspondingMTFieldType.TypeName = type.FullName;
                _uow.MTFieldTypeRepository.Add(correspondingMTFieldType);
            }
            return correspondingMTFieldType;
        }
    }
}
