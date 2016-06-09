using System.Collections.Generic;
using System.Data.Entity;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Interfaces;
using Hub.StructureMap;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Services
{
    [TestFixture]
    [Category("ActionService")]
    public class DatabaseIntegrityTests : BaseTest
    {
        //private IAction _action;
        //private IUnitOfWork _uow;
        //private FixtureData _fixtureData;
        private readonly IEnumerable<ActivityTemplateDO> _pr1Activities = new List<ActivityTemplateDO>() { new ActivityTemplateDO() { Name = "Write", Version = "1.0" }, new ActivityTemplateDO() { Name = "Read", Version = "1.0" } };
        private readonly IEnumerable<ActivityTemplateDO> _pr2Activities = new List<ActivityTemplateDO>() { new ActivityTemplateDO() { Name = "SQL Write", Version = "1.0" }, new ActivityTemplateDO() { Name = "SQL Read", Version = "1.0" } };

        [SetUp]
        public override void SetUp()
        {
            
           //note that for these tests we do NOT use the mock db. This has ramifications up on CI.
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
        }

        //this tests whether all the migrations are in place. if it's failing, check to see if they are, and make sure you're
        //trying to run the service, as the service runs this code itself and will crash if this test fails.
        [Test,Ignore]
        [Category("RequiresLocalDB")]
        public void Can_Initialize_Database()
        {
            var db = ObjectFactory.GetInstance<DbContext>();
            db.Database.Initialize(true);
        }

    }
}
