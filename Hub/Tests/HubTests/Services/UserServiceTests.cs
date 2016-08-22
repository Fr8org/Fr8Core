using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Services;
using Hub.StructureMap;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Services
{
    class UserServiceTests
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }
    }
}
