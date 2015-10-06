using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesTesting;
using pluginDocuSign.Actions;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using Core.Interfaces;
using Data.States;
using System.Threading.Tasks;


namespace pluginTests.pluginDocuSign.Actions
{
    [TestClass]
    public class Wait_For_DocuSign_Event_v1Tests : BaseTest
    {

        Wait_For_DocuSign_Event_v1 _wait_For_DocuSign_Event_v1;

        public Wait_For_DocuSign_Event_v1Tests()
        {
            base.SetUp();
            _wait_For_DocuSign_Event_v1 = new Wait_For_DocuSign_Event_v1();
        }

        [TestMethod,Ignore]
        public async Task Configure_ConfigurationRequestTypeIsInitial_ShouldCrateStorage()
        {
            //Arrange
            ActionDTO curActionDTO = FixtureData.TestActionDTO1();

            //Act
            var result = await _wait_For_DocuSign_Event_v1.Configure(curActionDTO);

            //Assert
            Assert.IsNotNull(result.CrateStorage);
            Assert.AreEqual(2, result.CrateStorage.CrateDTO.Count);
            Assert.AreEqual(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME, result.CrateStorage.CrateDTO[0].ManifestType);
            Assert.AreEqual(CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, result.CrateStorage.CrateDTO[1].ManifestType);



        }

        [TestMethod]
        public void Configure_ConfigurationRequestTypeIsFollowup_ShouldUpdateStorage()
        {
            //Arrange
            ActionDTO curActionDTO = FixtureData.TestActionDTO2();

            //Act
            var result = _wait_For_DocuSign_Event_v1.Configure(curActionDTO);

            //Assert
            Assert.AreEqual(2, result.Result.CrateStorage.CrateDTO.Count);
            Assert.AreEqual(CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_NAME, result.Result.CrateStorage.CrateDTO[1].ManifestType);


        }

        [TestMethod]
        public void Configure_ConfigurationRequestTypeIsFollowup_ShouldUpdateEventSubscription()
        {
            //Arrange
            ActionDTO curActionDTO = FixtureData.TestActionDTO3();

            //Act
            var result = _wait_For_DocuSign_Event_v1.Configure(curActionDTO);

            //Assert
            Assert.AreEqual(result.Result.CrateStorage.CrateDTO.Count, result.Result.CrateStorage.CrateDTO.Count);
            Assert.AreEqual(result.Result.CrateStorage.CrateDTO[1].ManifestType, result.Result.CrateStorage.CrateDTO[1].ManifestType);

        }

        [TestMethod]
        public void GetEnvelopeId_ParameterAsPayloadDTO_ReturnsEnvelopeInformation()
        {
            //Arrange
            PayloadDTO curPayloadDTO = FixtureData.PayloadDTO1();
            object[] parameters = new object[] { curPayloadDTO };

            //Act
            var result = (string)ClassMethod.Invoke(typeof(Wait_For_DocuSign_Event_v1), "GetEnvelopeId", parameters);

            //Assert
            Assert.AreEqual("EnvelopeIdValue", result);

        }

    }
}
