using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Core.Interfaces;
using Core.Managers;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.PluginRegistrations;
using Core.StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Wrappers;
using Moq;
using NUnit.Framework;
using StructureMap;
using StructureMap.Building;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Action = Core.Services.Action;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ActionService")]
    public class DatabaseIntegrityTests : BaseTest
    {
        private IAction _action;
        private IUnitOfWork _uow;
        private FixtureData _fixtureData;
        private readonly IEnumerable<ActionRegistrationDO> _pr1Actions = new List<ActionRegistrationDO>() { new ActionRegistrationDO() { ActionType = "Write", Version = "1.0" }, new ActionRegistrationDO() { ActionType = "Read", Version = "1.0" } };
        private readonly IEnumerable<ActionRegistrationDO> _pr2Actions = new List<ActionRegistrationDO>() { new ActionRegistrationDO() { ActionType = "SQL Write", Version = "1.0" }, new ActionRegistrationDO() { ActionType = "SQL Read", Version = "1.0" } };

        [SetUp]
        public override void SetUp()
        {
            
           //note that for these tests we do NOT use the mock db. This has ramifications up on CI.
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
        }

        //this tests whether all the migrations are in place
        [Test]
        public void Can_Initialize_Database()
        {
           // var curDbContext = new DbContext("name=DockyardDB");
            var db = ObjectFactory.GetInstance<DbContext>();
            db.Database.Initialize(true);
        }

    }
}
