using System;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;

namespace Data.Entities
{
    public class BaseDO : IBaseDO, ICreateHook, ISaveHook, IModifyHook
    {
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        public virtual void BeforeCreate()
        {
            if (CreateDate == default(DateTimeOffset))
                CreateDate = DateTimeOffset.Now;
        }

        public virtual void AfterCreate()
        {
        }

        public virtual void BeforeSave()
        {
            LastUpdated = DateTimeOffset.Now;
        }

        public virtual void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            this.DetectStateUpdates(originalValues, currentValues);
        }

    }
}
