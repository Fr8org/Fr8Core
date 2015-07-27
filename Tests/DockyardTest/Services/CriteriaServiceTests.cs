using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Data.Entities;
using DockyardTest.Fixtures;
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
            var envelopeDataList = FixtureData.CreateEnvelopeDataList();
            var results = new bool[2];
            var values = new string[] {envelopeDataList.First().Value, "__non-existing-value__"};
            for (var i = 0; i < 2; i++)
            {
                var criteriaObject = new { criteria = new[] { new { field = "Value", @operator = "Equals", value = values[i] } } };
                var criteriaString = JsonConvert.SerializeObject(criteriaObject);
                results[i] = _criteria.Evaluate(criteriaString, 0, "1", envelopeDataList);
            }
            Assert.IsTrue(results[0], "Criteria#Evaluate returned incorrect value.");
            Assert.IsFalse(results[1], "Criteria#Evaluate returned incorrect value.");
        }
    }
}
