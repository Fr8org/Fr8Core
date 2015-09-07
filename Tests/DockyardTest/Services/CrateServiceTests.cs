using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("CrateService")]
    public class CrateServiceTests : BaseTest
    {
        ICrate _crate;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _crate = ObjectFactory.GetInstance<ICrate>();
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public void Create_EmptyCrateStorageJSON_ThrowsException()
        {
            _crate.Create("");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(Newtonsoft.Json.JsonSerializationException))]
        public void AddCrate_InvalidJSON_ThrowsException()
        {
            _crate.Create("{test: 1}");
        }

        [Test]
        public void Create_ValidCrateJSON_ReturnsCrateDTO()
        {
            CrateDTO crateDTO = FixtureData.CrateDTO1();
            List<CrateDTO> crates = new List<CrateDTO>() { crateDTO };
            string currentCratesStorage = JsonConvert.SerializeObject(crates);

           CrateDTO createdCrateDTO = _crate.Create(currentCratesStorage);

           Assert.IsNotNull(createdCrateDTO);
           Assert.IsNotEmpty(createdCrateDTO.Id);
        }
    }
}
