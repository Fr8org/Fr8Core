using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Data.Entities;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("CriteriaService")]
    public class CriteriaServiceTests : BaseTest
    {
        private ICriteria _criteria;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _criteria = ObjectFactory.GetInstance<ICriteria>();
        }

        [Test]
        public void CriteriaService_CanApplyEqualCriterion()
        {
            const string value = "test value 1";
            var envelopeDataList = new List<EnvelopeDataDO>()
            {
                new EnvelopeDataDO() { Name = "test 1", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = value },
                new EnvelopeDataDO() { Name = "test 2", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 2" },
                new EnvelopeDataDO() { Name = "test 3", EnvelopeId = "1", RecipientId = "2", TabId = "0", Value = "test value 3" },
            };
            var criteriaObject = new {criteria = new[] {new {field = "Value", @operator = "Equals", value = value}}};
            var criteriaString = JsonConvert.SerializeObject(criteriaObject);
            var exists = _criteria.Evaluate(criteriaString, 0, "1", envelopeDataList);
            Assert.IsTrue(exists, "Criteria#Evaluate returned incorrect value.");
        }
    }
}
