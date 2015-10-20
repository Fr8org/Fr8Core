using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Data.Interfaces;
using Data.Entities;
using System.Collections.Generic;
using System.Reflection;
namespace Data.Infrastructure.MultiTenant
{
    public interface IMT_Field
    {
        List<Data.Entities.MT_Field> CreateList(IUnitOfWork _uow, List<PropertyInfo> curDataProperties, Data.Entities.MT_Object correspondingMTObject);

        //int? GetFieldColumnOffset(IUnitOfWork uow, string curMtFieldName, int curMtObjectId);

        //int GenerateFieldColumnOffset(IUnitOfWork uow, int curMtObjectId);

        //void Add(IUnitOfWork uow, Entities.MT_Field curMtField);
    }
}
