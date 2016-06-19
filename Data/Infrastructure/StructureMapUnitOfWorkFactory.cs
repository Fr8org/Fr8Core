using System;
using Data.Interfaces;
using StructureMap;

namespace Data.Infrastructure
{
    public class StructureMapUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IContainer _container;

        public StructureMapUnitOfWorkFactory(IContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            _container = container;
        }
        public IUnitOfWork Create()
        {
            return _container.GetInstance<IUnitOfWork>();
        }
    }
}
