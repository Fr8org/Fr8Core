using Moq;
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
using Newtonsoft.Json;
using System.Linq;
using Core.Enums;
using NUnit.Framework;
using pluginDocuSign.Tests.Fixtures;

namespace pluginDocuSign.Tests.Actions
{
    [TestFixture]
    [Category("pluginDocuSign")]
    public class Receive_DocuSign_Envelope_v1Tests : BaseTest
    {
        Receive_DocuSign_Envelope_v1 _extract_From_DocuSign_Envelope_v1;

        public Receive_DocuSign_Envelope_v1Tests()
        {
            base.SetUp();
            _extract_From_DocuSign_Envelope_v1 = new Receive_DocuSign_Envelope_v1();
        }

        [Test, Ignore("Vas, Introduced upstream actions logic to get the design time fields as part of DO-1300. This is invalid now")]
        public async Task Configure_ConfigurationRequestTypeIsInitial_ShouldCrateStorage()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                uow.RouteNodeRepository.Add(FixtureData.ConfigureTestActionTree());
                uow.SaveChanges();
                ActionDO curAction = FixtureData.ConfigureTestAction57();
                ActionDTO curActionDTO = Mapper.Map<ActionDTO>(curAction);
                curActionDTO.AuthToken = new AuthTokenDTO() { Token = JsonConvert.SerializeObject(PluginFixtureData.TestDocuSignAuthDTO1()) };

                Extract_From_DocuSign_Envelope_v1_Proxy curExtract_From_DocuSign_Envelope_v1_For_Testing = new Extract_From_DocuSign_Envelope_v1_Proxy();

                //Act
                var result = await curExtract_From_DocuSign_Envelope_v1_For_Testing.Configure(curActionDTO);

                //Assert
                Assert.IsNotNull(result.CrateStorage);
                Assert.AreEqual(2, result.CrateStorage.CrateDTO.Count);
                Assert.AreEqual(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME, result.CrateStorage.CrateDTO[0].ManifestType);
                Assert.AreEqual(CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, result.CrateStorage.CrateDTO[1].ManifestType);
            }
        }

        [Test]
        public void GetEnvelopeId_ParameterAsPayloadDTO_ReturnsEnvelopeInformation()
        {
            //Arrange
            PayloadDTO curPayloadDTO = FixtureData.PayloadDTO2();
            object[] parameters = new object[] { curPayloadDTO };

            //Act
            var result = (string)ClassMethod.Invoke(typeof(Receive_DocuSign_Envelope_v1), "GetEnvelopeId", parameters);

            //Assert
            Assert.AreEqual("EnvelopeIdValue", result);

        }

        [Test]
        public void GetFields_ActionDTOAsParameter_ReturnsFieldsInformation()
        {
            //Arrange
            ActionDTO curActionDTO = FixtureData.CreateStandardDesignTimeFields();
            curActionDTO.AuthToken = new AuthTokenDTO() { Token = JsonConvert.SerializeObject(PluginFixtureData.TestDocuSignAuthDTO1()) };
            object[] parameters = new object[] { curActionDTO };

            //Act
            var result = (List<FieldDTO>)ClassMethod.Invoke(typeof(Receive_DocuSign_Envelope_v1), "GetFields", parameters);

            //Assert
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("Text 5", result[0].Key);
            Assert.AreEqual("Text 8", result[1].Key);
            Assert.AreEqual("Doctor", result[2].Key);
            Assert.AreEqual("Condition", result[3].Key);
        }

        [Test]
        public void CreateActionPayload_ReturnsFieldsValue()
        {
            //Arrange
            ActionDTO curActionDTO = FixtureData.CreateStandardDesignTimeFields();
            curActionDTO.AuthToken = new AuthTokenDTO() { Token = JsonConvert.SerializeObject(PluginFixtureData.TestDocuSignAuthDTO1()) };

            //Act
            var result = _extract_From_DocuSign_Envelope_v1.CreateActionPayload(curActionDTO, "8fcb42d3-1572-44eb-acb1-0fffa4ca65de");

            //Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Dohemann", result.FirstOrDefault(x => x.Key == "Doctor").Value);
            Assert.AreEqual("Gout", result.FirstOrDefault(x => x.Key == "Condition").Value);
            Assert.AreEqual("test", result.FirstOrDefault(x => x.Key == "Text 5").Value);
        }

    }
    public class Extract_From_DocuSign_Envelope_v1_Proxy : Receive_DocuSign_Envelope_v1
    {
        private readonly IRouteNode _activity;

        public Extract_From_DocuSign_Envelope_v1_Proxy()
        {
            _activity = ObjectFactory.GetInstance<IRouteNode>();
        }

        protected async override Task<List<CrateDTO>> GetCratesByDirection(int activityId, string manifestType, GetCrateDirection direction)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var actionDO = uow.ActionRepository.GetByKey(activityId);
                var upstreamActions = _activity
                    .GetUpstreamActivities(uow, actionDO)
                    .OfType<ActionDO>()
                    .Select(x => Mapper.Map<ActionDTO>(x))
                    .ToList();

                var curCrates = new List<CrateDTO>();

                foreach (var curAction in upstreamActions)
                {
                    curCrates.AddRange(Crate.GetCratesByManifestType(manifestType, curAction.CrateStorage).ToList());
                }

                return await Task.FromResult(curCrates);
            }
        }
    }
}