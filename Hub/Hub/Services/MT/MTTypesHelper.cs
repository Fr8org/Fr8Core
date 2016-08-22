using System;
using System.Collections.Generic;
using System.Linq;
using Data.Interfaces;
using Data.Repositories.MultiTenant;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;

namespace Hub.Services.MT
{
    public class MTTypesHelper
    {
        public static IEnumerable<FieldDTO> GetFieldsByTypeId(IUnitOfWork uow, Guid typeId, AvailabilityType availability = AvailabilityType.NotSet)
        {
            return uow.MultiTenantObjectRepository
                .ListTypePropertyReferences(typeId)
                .OrderBy(x => x.Name)
                .Select(x => new FieldDTO()
                {
                    Name = x.Name,
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
