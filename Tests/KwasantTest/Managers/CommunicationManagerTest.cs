using System;
using Data.Infrastructure;
using Data.Interfaces;
using Core.Managers;
using Core.Services;
using Core.StructureMap;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities;
using System.Linq;
namespace KwasantTest.Managers
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
