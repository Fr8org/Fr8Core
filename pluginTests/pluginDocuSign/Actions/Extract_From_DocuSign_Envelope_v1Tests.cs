using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesTesting;
using pluginDocuSign.Actions;
using Data.Interfaces.DataTransferObjects;
using Core.Interfaces;
using UtilitiesTesting.Fixtures;
using StructureMap;
using Data.Interfaces;
using Data.Entities;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PluginBase.BaseClasses;
using Newtonsoft.Json;
using System.Net.Http;

namespace pluginTests.pluginDocuSign.Actions
{
    [TestClass]
    public class Extract_From_DocuSign_Envelope_v1Tests : BaseTest
    {
        Extract_From_DocuSign_Envelope_v1 _extract_From_DocuSign_Envelope_v1;

        public Extract_From_DocuSign_Envelope_v1Tests()
        {
            base.SetUp();
            _extract_From_DocuSign_Envelope_v1 = new Extract_From_DocuSign_Envelope_v1();
        }

        [TestMethod]
        public void Configure_ConfigurationRequestTypeIsInitial_ShouldCrateStorage()
        {

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange

                uow.ActivityRepository.Add(FixtureData.ConfigureTestActionTree());
                uow.SaveChanges();

                ActionDO curAction = FixtureData.ConfigureTestAction57();

                //Act
                var result = _extract_From_DocuSign_Envelope_v1.Configure(Mapper.Map<ActionDTO>(curAction));

                //Assert
                Assert.IsNotNull(result.CrateStorage);
                Assert.AreEqual(2, result.CrateStorage.CrateDTO.Count);
                Assert.AreEqual(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME, result.CrateStorage.CrateDTO[0].ManifestType);
                Assert.AreEqual(CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, result.CrateStorage.CrateDTO[1].ManifestType);

            }
        }

        [TestMethod]
        public void GetEnvelopeId_ParameterAsPayloadDTO_ReturnsEnvelopeInformation()
        {
            //Arrange
            PayloadDTO curPayloadDTO = FixtureData.PayloadDTO2();
            object[] parameters = new object[] { curPayloadDTO };

            //Act
            var result = (string)ClassMethod.Invoke(typeof(Extract_From_DocuSign_Envelope_v1), "GetEnvelopeId", parameters);

            //Assert
            Assert.AreEqual("EnvelopeIdValue", result);

        }


        [TestMethod]
        public void GetFields_ActionDTOAsParameter_ReturnsFieldsInformation()
        {
            //Arrange
            ActionDTO curActionDTO = FixtureData.CreateStandardDesignTimeFields();
            object[] parameters = new object[] { curActionDTO };

            //Act
            var result = (List<FieldDTO>)ClassMethod.Invoke(typeof(Extract_From_DocuSign_Envelope_v1), "GetFields", parameters);

            //Assert
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("Text 5", result[0].Key);
            Assert.AreEqual("Text 8", result[1].Key);
            Assert.AreEqual("Doctor", result[2].Key);
            Assert.AreEqual("Condition", result[3].Key);
        }

        [TestMethod]
        public void CreateActionPayload_ReturnsFieldsValue()
        {
            //Arrange
            ActionDTO curActionDTO = FixtureData.CreateStandardDesignTimeFields();

            //Act
            var result = _extract_From_DocuSign_Envelope_v1.CreateActionPayload(curActionDTO, "f02c3d55-f6ef-4b2b-b0a0-02bf64ca1e09");

            //Assert
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("Smathers", result[0].Value);
            Assert.AreEqual("Golden Oriole", result[1].Value);
            Assert.AreEqual("Johnson", result[2].Value);
            Assert.AreEqual("Marthambles", result[3].Value);

        }
    }
}
