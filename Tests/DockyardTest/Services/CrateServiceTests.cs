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
        public void Create_CreateNewCrate_ReturnsCrateDTO()
        {
            CrateDTO crateDTO = _crate.Create("Label 1", "Content 1");
            Assert.NotNull(crateDTO);
            Assert.IsNotEmpty(crateDTO.Id);
        }

        [Test]
        public void CanGetElementByKey()
        {
            var crates = new[]
            {
                _crate.Create("Label 1", "[ { name: \"connection_string\", value: \"TestConnectionStringA\" }, { name: \"Another Key\", value: \"Another Value\" } ]"),
                _crate.Create("Label 2", "[ { name: \"connection_string\", value: \"TestConnectionStringC\" }, { name: \"Another Key\", value: \"Another Value\" } ]"),
            };
            var elements = _crate.GetElementByKey(crates, "connection_string", "name").ToArray();
            var values = elements.Select(e => (string)e["value"]).ToArray();
            Assert.NotNull(elements);
            Assert.AreEqual(2, elements.Length);
            Assert.Contains("TestConnectionStringA", values);
            Assert.Contains("TestConnectionStringC", values);
        }

        [Test]
        public void CanDeserealizeStandardConfigurationControls()
        {
            // Arrange
            var _crate = ObjectFactory.GetInstance<ICrate>();
            var controls = FixtureData.SampleConfigurationControls();
            var curCrateDTO = _crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", controls.ToArray());

            // Act
            var standardConfigurationControlsMS = _crate.GetStandardConfigurationControls(curCrateDTO);

            // Assert
            Assert.NotNull(standardConfigurationControlsMS);
            Assert.NotNull(standardConfigurationControlsMS.Controls);
            Assert.AreEqual(2, standardConfigurationControlsMS.Controls.Count);
            Assert.IsTrue(typeof (DropDownListControlDefinitionDTO) == standardConfigurationControlsMS.Controls[0].GetType());
            Assert.IsTrue(typeof (RadioButtonGroupControlDefinitionDTO) == standardConfigurationControlsMS.Controls[1].GetType());

            var radiobuttonGroup = standardConfigurationControlsMS.Controls[1] as RadioButtonGroupControlDefinitionDTO;
            Assert.NotNull(radiobuttonGroup.Radios);
            Assert.AreEqual(2, radiobuttonGroup.Radios.Count());
            Assert.NotNull(radiobuttonGroup.Radios[0].Controls);
            Assert.NotNull(radiobuttonGroup.Radios[1].Controls);
            Assert.AreEqual(1, radiobuttonGroup.Radios[0].Controls.Count);
            Assert.AreEqual(1, radiobuttonGroup.Radios[1].Controls.Count);

            Assert.AreEqual(typeof (TextBoxControlDefinitionDTO), radiobuttonGroup.Radios[0].Controls[0].GetType());
            Assert.AreEqual(typeof (DropDownListControlDefinitionDTO), radiobuttonGroup.Radios[1].Controls[0].GetType());
        }

        //[Test]
        //public void CanSerializeStandardConfigurationControls()
        //{
        //    // Arrange
        //    var _crate = ObjectFactory.GetInstance<ICrate>();
        //    var controls = FixtureData.AllConfigurationControls();
        //    var curCrateDTO = _crate.CreateStandardConfigurationControlsCrate("Configuration_Controls", controls.ToArray());
        //}
    }
}
