using Data.Interfaces;
using Hub.Interfaces;
using HubTests.Fixtures;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesTesting;

namespace HubTests.Services
{
    [TestFixture]
    [Category("PlanDescription")]
    public class PlanTemplatesTests : BaseTest
    {
        private IPlanTemplates _planDescription;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _planDescription = ObjectFactory.GetInstance<IPlanTemplates>();
        }

        [Test]
        public void PlanDescriptionSaves()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
               //TODO:
            }
        }
    }
}
