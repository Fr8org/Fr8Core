using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using Data.Interfaces;
using Data.Repositories.MultiTenant;
using Fr8Data.DataTransferObjects;
using Fr8Data.States;

namespace TerminalBase.Services.MT
{
    public class MTTypesHelper
    {
        public static IEnumerable<FieldDTO> GetFieldsByTypeId(
            Guid typeId, AvailabilityType availability = AvailabilityType.NotSet)
        {
            var fields = new Dictionary<string, string>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return GetFieldsByTypeId(uow, typeId, availability);
            }
        }

        public static IEnumerable<FieldDTO> GetFieldsByTypeId(
            IUnitOfWork uow, Guid typeId, AvailabilityType availability = AvailabilityType.NotSet)
        {
            return uow.MultiTenantObjectRepository
                .ListTypePropertyReferences(typeId)
                .OrderBy(x => x.Name)
                .Select(x => new FieldDTO()
                {
                    Key = x.Name,
                    FieldType = GetFieldType(x),
                    Availability = availability
                })
                .ToList();
        }

        public static string GetFieldType(MtTypePropertyReference propReference)
        {
            if (propReference.PropertyClrType == typeof(DateTime)
                || propReference.PropertyClrType == typeof(DateTime?))
            {
                return FieldType.Date;
            }
            else
            {
                return FieldType.String;
            }
        }
    }
}
