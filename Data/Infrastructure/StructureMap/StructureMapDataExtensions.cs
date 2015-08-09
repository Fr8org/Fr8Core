using System;
using Data.Interfaces;
using StructureMap;

namespace Data.Infrastructure.StructureMap
{
    public static class StructureMapDataExtensions
    {
        public static void InUnitOfWork(this IUnitOfWorkAwareComponent component, Action<IUnitOfWork> action)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                action.Invoke(uow);
                uow.SaveChanges();
            }
        }

        public static T InUnitOfWork<T>(this IUnitOfWorkAwareComponent component, Func<IUnitOfWork, T> action)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var result = action.Invoke(uow);
                uow.SaveChanges();

                return result;
            }
        }
    }
}
