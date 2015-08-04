using System;
using Core.Interfaces;
using Data.Interfaces;
using StructureMap;

namespace Core.Helper
{
    public static class UnitOfWorkExtensions
    {
        public static T Using<T>(this IService unitOfWork, Func<IUnitOfWork,T> func)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return func(uow);
            }
        }
        public static void Using(this IService unitOfWork, Action<IUnitOfWork> action)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                action(uow);
            }
        }
    }
}