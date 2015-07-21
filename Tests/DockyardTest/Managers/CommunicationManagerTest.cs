using System;
using System.Linq;
using Data.Infrastructure;
using Data.Interfaces;
using Core.Managers;
using Core.Services;
using Core.StructureMap;
using DockyardTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using TestCommons;
using Utilities;

namespace DockyardTest.Managers
{
    [TestFixture]
    public class CommunicationManagerTest : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

       
    }
}
