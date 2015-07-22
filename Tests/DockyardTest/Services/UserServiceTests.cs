using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Core.Services;
using Core.StructureMap;
using NUnit.Framework;
using StructureMap;
using TestCommons.Fixtures;

namespace DockyardTest.Services
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
