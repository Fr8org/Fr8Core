
﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Crates;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using Hub.Enums;
using Hub.Interfaces;
using Utilities.Configuration.Azure;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using terminalDocuSign.Actions;
using terminalDocuSign.Infrastructure.AutoMapper;
using terminalDocuSign.Tests.Fixtures;
using Hub.Managers;

namespace terminalDocuSign.Tests.Actions
{
    [TestFixture]
    [Category("terminalDocuSign")]
    public class Receive_DocuSign_Envelope_v1Tests : BaseTest
    {
        Receive_DocuSign_Envelope_v1 _extract_From_DocuSign_Envelope_v1;
        ICrateManager _crate;
        public Receive_DocuSign_Envelope_v1Tests()
        {
            base.SetUp();
            CloudConfigurationManager.RegisterApplicationSettings(new AppSettingsFixture());

            TerminalDataAutoMapperBootStrapper.ConfigureAutoMapper();

            _extract_From_DocuSign_Envelope_v1 = new Receive_DocuSign_Envelope_v1();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

//        [Test, Ignore("Vas, Introduced upstream actions logic to get the design time fields as part of DO-1300. This is invalid now")]
//        public async Task Configure_ConfigurationRequestTypeIsInitial_ShouldCrateStorage()
//        {
//            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
//            {
//                //Arrange
//                uow.RouteNodeRepository.Add(FixtureData.ConfigureTestActionTree());
//                uow.SaveChanges();
//                ActionDO curAction = FixtureData.ConfigureTestAction57();
//                ActionDTO curActionDTO = Mapper.Map<ActionDTO>(curAction);
//                curActionDTO.AuthToken = new AuthTokenDTO() { Token = JsonConvert.SerializeObject(PluginFixtureData.TestDocuSignAuthDTO1()) };
//
//                Extract_From_DocuSign_Envelope_v1_Proxy curExtract_From_DocuSign_Envelope_v1_For_Testing = new Extract_From_DocuSign_Envelope_v1_Proxy();
//
//                //Act
//                var result = await curExtract_From_DocuSign_Envelope_v1_For_Testing.Configure(curActionDTO);
//
//                //Assert
//                Assert.IsNotNull(result.CrateStorage);
//                Assert.AreEqual(2, result.CrateStorage.CrateDTO.Count);
//                Assert.AreEqual(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME, result.CrateStorage.CrateDTO[0].ManifestType);
//                Assert.AreEqual(CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, result.CrateStorage.CrateDTO[1].ManifestType);
//            }
//        }

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
            curActionDTO.AuthToken = new AuthorizationTokenDTO() { Token = JsonConvert.SerializeObject(TerminalFixtureData.TestDocuSignAuthDTO1()) };
            var curActionDO = Mapper.Map<ActionDO>(curActionDTO);
            object[] parameters = new object[] { curActionDO };

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
            curActionDTO.AuthToken = new AuthorizationTokenDTO() { Token = JsonConvert.SerializeObject(TerminalFixtureData.TestDocuSignAuthDTO1()) };
            var curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(curActionDTO.AuthToken);
            var curActionDO = Mapper.Map<ActionDO>(curActionDTO);
            //Act
            var result = _extract_From_DocuSign_Envelope_v1.CreateActionPayload(curActionDO, curAuthTokenDO, "8fcb42d3-1572-44eb-acb1-0fffa4ca65de");


            //Assert
            Assert.AreEqual(3, result.PayloadObjects[0].PayloadObject.Count());
            Assert.AreEqual("Dohemann", result.PayloadObjects[0].PayloadObject.FirstOrDefault(x => x.Key == "Doctor").Value);
            Assert.AreEqual("Gout", result.PayloadObjects[0].PayloadObject.FirstOrDefault(x => x.Key == "Condition").Value);
            Assert.AreEqual("test", result.PayloadObjects[0].PayloadObject.FirstOrDefault(x => x.Key == "Text 5").Value);
        }

    }
    public class Extract_From_DocuSign_Envelope_v1_Proxy : Receive_DocuSign_Envelope_v1
    {
        private readonly IRouteNode _activity;

        public Extract_From_DocuSign_Envelope_v1_Proxy()
        {
            _activity = ObjectFactory.GetInstance<IRouteNode>();
        }

        public async  Task<List<Crate>> GetCratesByDirection(int activityId, string manifestType, GetCrateDirection direction)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var actionDO = uow.ActionRepository.GetByKey(activityId);
                var upstreamActions = _activity
                    .GetUpstreamActivities(uow, actionDO)
                    .OfType<ActionDO>()
                    .Select(x => Mapper.Map<ActionDTO>(x))
                    .ToList();

                var curCrates = new List<Crate>();

                foreach (var curAction in upstreamActions)
                {
                    curCrates.AddRange(Crate.FromDto(curAction.CrateStorage).Where(x=>x.ManifestType.Type == manifestType).ToList());
                }

                return await Task.FromResult(curCrates);
            }
        }
    }
}