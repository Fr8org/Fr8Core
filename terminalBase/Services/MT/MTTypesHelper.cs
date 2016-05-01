using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Repositories.MultiTenant;

namespace TerminalBase.Services.MT
{
    public class MTTypesHelper
    {
        public static IEnumerable<FieldDTO> GetFieldsByTypeId(Guid typeId)
        {
            var fields = new Dictionary<string, string>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return GetFieldsByTypeId(uow, typeId);
            }
        }

        public static IEnumerable<FieldDTO> GetFieldsByTypeId(IUnitOfWork uow, Guid typeId)
        {
            return uow.MultiTenantObjectRepository
                .ListTypePropertyReferences(typeId)
                .OrderBy(x => x.Name)
                .Select(x => new FieldDTO()
                {
                    Key = x.Name,
                    FieldType = GetFieldType(x)
                })
                .ToList();
        }

        public static string GetFieldType(MtTypePropertyReference propReference)
        {
            if (propReference.PropertyClrType == typeof(DateTime)
                || propReference.PropertyClrType == typeof(DateTime?))
            {
                return FieldType2.Date;
            }
            else
            {
                return FieldType2.String;
            }
        }
    }
}
