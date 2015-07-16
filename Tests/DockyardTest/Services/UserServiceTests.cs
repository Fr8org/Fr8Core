using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Core.Services;
using Core.StructureMap;
using DockyardTest.Fixtures;
using NUnit.Framework;
using StructureMap;

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
