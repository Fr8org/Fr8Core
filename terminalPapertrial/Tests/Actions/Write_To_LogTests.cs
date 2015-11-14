
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using NUnit.Framework;
using terminalPapertrial.Actions;
using UtilitiesTesting;

namespace terminalPapertrial.Tests.Actions
{
    [TestFixture]
    [Category("terminalPapertrialActions")]
    public class Write_To_LogTests : BaseTest
    {
        private Write_To_Log_v1 _action_under_test;

        public override void SetUp()
        {
            base.SetUp();
            _action_under_test = new Write_To_Log_v1();
        }

        [Test]
        public async Task Configure_InitialConfigurationResponse_ShourldReturn_OneConfigControlsCrate()
        {
            //Act
            var result = await _action_under_test.Configure(new ActionDO());
            ActionDTO resultActionDTO = Mapper.Map<ActionDTO>(result);

            //Assert
            var crateStorage = new CrateManager().FromDto(resultActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count, "Initial configuration is failed for Write To Log action in Papertrial");

            var configControlCrates = crateStorage.CratesOfType<StandardConfigurationControlsCM>().ToList();
            Assert.AreEqual(1, configControlCrates.Count, "More than one configuration controls are avaialbe for Write To Log action");

            var targetUrlControl = configControlCrates.First().Content.Controls[0];
            Assert.IsNotNull(targetUrlControl, "Papertrial target URL control is not configured.");
            Assert.AreEqual("TargetUrlTextBox", targetUrlControl.Name, "Papertrial target URL control is not configured correctly");
        }

        [Test]
        public async Task Configure_FollowUpConfigurationResponse_ShourldReturn_OneConfigControlsCrate()
        {
            //Arrange
            ActionDO testAction = new ActionDO();
            await _action_under_test.Configure(testAction);

            //Act
            var result = await _action_under_test.Configure(testAction);
            ActionDTO resultActionDTO = Mapper.Map<ActionDTO>(result);

            //Assert
            var crateStorage = new CrateManager().FromDto(resultActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count, "Followup configuration is failed for Write To Log action in Papertrial");

            var configControlCrates = crateStorage.CratesOfType<StandardConfigurationControlsCM>().ToList();
            Assert.AreEqual(1, configControlCrates.Count, "More than one configuration controls are avaialbe for Write To Log action");

            var targetUrlControl = configControlCrates.First().Content.Controls[0];
            Assert.IsNotNull(targetUrlControl, "Papertrial target URL control is not configured.");
            Assert.AreEqual("TargetUrlTextBox", targetUrlControl.Name, "Papertrial target URL control is not configured correctly");
        }
    }
}