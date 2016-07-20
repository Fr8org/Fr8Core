using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Data.Repositories.MultiTenant;
using Data.Repositories.SqlBased;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalFr8CoreTests.Fixtures;
using StructureMap;

namespace terminalFr8CoreTests.Integration
{
    [Explicit]
    public class SaveToFr8Warehouse_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        private void AssertConfigureControls(StandardConfigurationControlsCM control)
        {
            Assert.AreEqual(1, control.Controls.Count);

            Assert.IsTrue(control.Controls[0] is UpstreamCrateChooser);
            Assert.AreEqual("Store which crates?", control.Controls[0].Label);
            Assert.AreEqual("UpstreamCrateChooser", control.Controls[0].Name);
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test]
        public async Task SaveToFr8Warehouse_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = FixtureData.SaveToFr8Wareouse_InitialConfiguration_Fr8DataDTO();

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count);

            AssertConfigureControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        /// <summary>
        /// Test run-time for action Run().
        /// </summary>
        [Test]
        public async Task SaveToFr8Warehouse_Run()
        {
            var runUrl = GetTerminalRunUrl();
            var dataDTO = FixtureData.SaveToFr8Wareouse_InitialConfiguration_Fr8DataDTO();
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();

            dataDTO.ActivityDTO.AuthToken = new AuthorizationTokenDTO
            {
                UserId = "TestUser",
            };

            using (var storage = crateManager.GetUpdatableStorage(dataDTO.ActivityDTO))
            {
                storage.Clear();

                var configControlCm = new StandardConfigurationControlsCM();

                var docusignEnvelope = new DropDownList
                {
                    selectedKey = MT.DocuSignEnvelope.ToString(),
                    Value = ((int)MT.DocuSignEnvelope).ToString(),
                    Name = "UpstreamCrateChooser_mnfst_dropdown_0",
                };

                configControlCm.Controls.Add(new UpstreamCrateChooser
                {
                    Name = "UpstreamCrateChooser",
                    SelectedCrates = new List<CrateDetails>
                    {
                        new CrateDetails {ManifestType = docusignEnvelope},
                    }
                });

                storage.Add(Fr8.Infrastructure.Data.Crates.Crate.FromContent("Configuration_Controls", configControlCm));
            }

            string envelopeId = "testEnvelope_" + Guid.NewGuid().ToString("N");

            DateTime time = new DateTime(2016, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc);
            DateTime time2 = new DateTime(2016, 1, 2, 3, 4, 5, 6, DateTimeKind.Local);

            AddPayloadCrate(
                dataDTO,
                new DocuSignEnvelopeCM
                {
                    EnvelopeId = envelopeId,
                    Status = "Sent",
                    SentDate = time,
                    CreateDate = time2,
                    ExternalAccountId = "TestUser"
                }
                ,
                "TestEnvelope"
                );

            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            var responsePayloadDTO = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            Assert.NotNull(responsePayloadDTO);
            Assert.NotNull(responsePayloadDTO.CrateStorage);
            Assert.NotNull(responsePayloadDTO.CrateStorage.Crates);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var result = uow.MultiTenantObjectRepository.Query<DocuSignEnvelopeCM>("TestUser", x => x.EnvelopeId == envelopeId).FirstOrDefault();
                Assert.NotNull(result, DumpDebugInfo());

                Assert.NotNull(result.SentDate, "Sent date is null");
                Assert.NotNull(result.CreateDate, "Sent date is null");

                Assert.AreEqual(time.ToUniversalTime(), result.SentDate.Value.ToUniversalTime(), "Invalid SentDate of stored envelope. " + DumpDebugInfo());
                Assert.AreEqual(time2.ToUniversalTime(), result.CreateDate.Value.ToUniversalTime(), "Invalid CreateDate of stored envelope. " + DumpDebugInfo());
                Assert.AreEqual("Sent", result.Status, "Invalid status of stored envelope. " + DumpDebugInfo());
            }
        }

        private string DisplayTypeResolution<T>()
        {
            try
            {
                var type = ObjectFactory.GetInstance<T>();
                return typeof(T).Name + " is resolved to " + type.GetType().FullName;
            }
            catch (Exception)
            {
                return $"failed to resolve {typeof(T).Name}";
            }
        }

        private string ResolveConnectionString()
        {
            try
            {
                var provider = ObjectFactory.GetInstance<ISqlConnectionProvider>();
                return provider.ConnectionInfo?.ToString() ?? "No connection info available";
            }
            catch
            {
                return "Unable to resolve connection string";
            }
        }

        private string DumpDebugInfo()
        {
            StringBuilder debugInfo = new StringBuilder("\n");
            string cs = ResolveConnectionString();

            debugInfo.AppendLine(DisplayTypeResolution<ISqlConnectionProvider>());
            debugInfo.AppendLine(DisplayTypeResolution<IMtTypeStorageProvider>());
            debugInfo.AppendLine($"Current connection string for MT is: {Fr8.Infrastructure.Utilities.MiscUtils.MaskPassword(cs)}");

            return debugInfo.ToString();
        }
    }
}
