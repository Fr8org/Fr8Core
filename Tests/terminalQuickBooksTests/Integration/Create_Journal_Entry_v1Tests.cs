using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;
using terminalQuickBooks.Controllers;
using terminalQuickBooks.Services;
using terminalQuickBooksTests.Fixtures;

namespace terminalQuickBooksTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    class Create_Journal_Entry_v1Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalQuickBooks"; }
        }
        [Test, Category("Integration.terminalQuickBooks")]
        public async void Create_Journal_Entry_Configuration_Check_With_No_Upstream_Crate()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Create_Journal_Entry_v1_InitialConfiguration_ActionDTO();
            //Act
            var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );
            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);
            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            var curTextBlock = (TextBlock)crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single().Controls[0];
            Assert.AreEqual("Create Journal Entry", curTextBlock.Label);
            Assert.AreEqual("In order to Create a Journal Entry, an upstream action needs to provide a StandardAccountingTransactionCM.", curTextBlock.Value);
            Assert.AreEqual("alert alert-warning", curTextBlock.CssClass);
        }
        [Test, Category("Integration.terminalQuickBooks")]
        public async void Create_Journal_Entry_Configuration_Check_With_Upstream_Crate()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();
            var requestActionDTO = HealthMonitor_FixtureData.Create_Journal_Entry_v1_InitialConfiguration_ActionDTO();
            var curStandAccTransCrate = HealthMonitor_FixtureData.GetAccountingTransactionCM();
            AddUpstreamCrate(requestActionDTO, curStandAccTransCrate);
            //Act
            var responseActionDTO = await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );
            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);
            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            var curTextBlock = (TextBlock)crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single().Controls[0];
            Assert.AreEqual("Create a Journal Entry", curTextBlock.Label);
            Assert.AreEqual("This Action doesn't require any configuration.", curTextBlock.Value);
            Assert.AreEqual("well well-lg", curTextBlock.CssClass);
        }
    }
}
