using UtilitiesTesting;
using terminalDocuSign.Actions;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using Data.Interfaces;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json;
using terminalDocuSign.Tests.Fixtures;
using terminalDocuSign.Infrastructure.StructureMap;
using terminalDocuSign.Infrastructure.AutoMapper;
using Utilities.Configuration.Azure;


namespace terminalDocuSign.Tests.Actions
{
    [TestFixture]
    [Category("terminalDocuSign")]
    public class Monitor_DocuSignTests : BaseTest
    {
        Monitor_DocuSign_v1 _monitor_DocuSign;

        public Monitor_DocuSignTests()
        {
            base.SetUp();
            PluginDocuSignMapBootstrapper.ConfigureDependencies(Hub.StructureMap.StructureMapBootStrapper.DependencyType.TEST);
            PluginDataAutoMapperBootStrapper.ConfigureAutoMapper();
            CloudConfigurationManager.RegisterApplicationSettings(new AppSettingsFixture());

            _monitor_DocuSign = new Monitor_DocuSign_v1();
        }

        [Test]
        public async Task Configure_ConfigurationRequestTypeIsInitial_ShouldCrateStorage()
        {
            //Arrange
            ActionDTO curActionDTO = FixtureData.TestActionDTO1();
            curActionDTO.AuthToken = new AuthTokenDTO() { Token = JsonConvert.SerializeObject(PluginFixtureData.TestDocuSignAuthDTO1()) };

            //Act
            var result = await _monitor_DocuSign.Configure(curActionDTO);

            //Assert
            Assert.IsNotNull(result.CrateStorage);
            Assert.AreEqual(4, result.CrateStorage.CrateDTO.Count);
            Assert.AreEqual(CrateManifests.STANDARD_CONF_CONTROLS_MANIFEST_NAME, result.CrateStorage.CrateDTO[0].ManifestType);
            Assert.AreEqual(CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, result.CrateStorage.CrateDTO[1].ManifestType);

            //DO-1300 states Initial configuration response should add the standard design time fields with envelope ID
            Assert.AreEqual(CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, result.CrateStorage.CrateDTO[2].ManifestType);
            Assert.AreEqual("DocuSign Event Fields", result.CrateStorage.CrateDTO[2].Label);

            //NOTE:DO-1236 states - initial configuration response should add the standard event subscription
            Assert.AreEqual(CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_NAME, result.CrateStorage.CrateDTO[3].ManifestType);
        }

        [Test]
        public void Configure_ConfigurationRequestTypeIsFollowup_ShouldUpdateStorage()
        {
            //NOTE: acc to DO-1236 - not required anymore
            ////Arrange   
            //ActionDTO curActionDTO = FixtureData.TestActionDTO2();
            //curActionDTO.AuthToken = new AuthTokenDTO() { Token = JsonConvert.SerializeObject(PluginFixtureData.TestDocuSignAuthDTO1()) };

            ////Act
            //var result = _monitor_DocuSign.Configure(curActionDTO);

            ////Assert
            //Assert.AreEqual(2, result.Result.CrateStorage.CrateDTO.Count);
            //Assert.AreEqual(CrateManifests.STANDARD_EVENT_SUBSCRIPTIONS_NAME, result.Result.CrateStorage.CrateDTO[0].ManifestType);
        }

        [Test, Ignore]
        public void Configure_ConfigurationRequestTypeIsFollowup_ShouldUpdateEventSubscription()
        {
            //Arrange
            ActionDTO curActionDTO = FixtureData.TestActionDTO3();
            curActionDTO.AuthToken = new AuthTokenDTO() { Token = JsonConvert.SerializeObject(PluginFixtureData.TestDocuSignAuthDTO1()) };

            //Act
            var result = _monitor_DocuSign.Configure(curActionDTO);

            //Assert
            Assert.AreEqual(result.Result.CrateStorage.CrateDTO.Count, result.Result.CrateStorage.CrateDTO.Count);
            Assert.AreEqual(result.Result.CrateStorage.CrateDTO[1].ManifestType, result.Result.CrateStorage.CrateDTO[1].ManifestType);
        }

        [Test]
        public void GetEnvelopeId_ParameterAsPayloadDTO_ReturnsEnvelopeInformation()
        {
            //Arrange
            PayloadDTO curPayloadDTO = FixtureData.PayloadDTO1();
            object[] parameters = new object[] { curPayloadDTO, "EnvelopeId" };

            //Act
            var result = (string)ClassMethod.Invoke(typeof(Monitor_DocuSign_v1), "GetValueForKey", parameters);

            //Assert
            Assert.AreEqual("EnvelopeIdValue", result);
        }

    }
}