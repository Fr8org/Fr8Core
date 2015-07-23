using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Core.StructureMap;
using Web.App_Start;
using NUnit.Framework;
using StructureMap;

namespace DockyardTest
{
    [TestFixture]
    public class BaseTest
    {

        [SetUp]
        public virtual void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            MockedDBContext.WipeMockedDatabase();
            AutoMapperBootStrapper.ConfigureAutoMapper();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>()) //Get the seeding done first
                uow.SaveChanges();
        }
    }
}
