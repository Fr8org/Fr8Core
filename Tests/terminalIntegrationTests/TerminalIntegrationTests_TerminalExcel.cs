using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Crates;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Moq;
using terminalIntegrationTests.Fixtures;
using terminalExcel.Actions;
using terminalExcel.Fixtures;
using terminalUtilities.Excel;

namespace terminalIntegrationTests
{
    public partial class TerminalIntegrationTests
    {
        [Test, Ignore]
        public async Task TerminalExcel_CallExtractData_Execute()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = new PlanDO()
                {
                    Id = UtilitiesTesting.Fixtures.FixtureData.TestParentPlanID(),
                    Name = "1",
                    PlanState = PlanState.Active
                };

                uow.PlanRepository.Add(plan);

                uow.ContainerRepository.Add(new ContainerDO()
                {
                    Id = UtilitiesTesting.Fixtures.FixtureData.TestContainer_Id_1(),
                    Plan = plan,
                    CrateStorage = _crateManager.EmptyStorageAsStr(),
                    State = State.Executing
                });

                uow.SaveChanges();
            }

            var crate = new CrateManager();

            var bytesFromExcel = TerminalFixtureData.TestExcelData();
            var columnHeaders = ExcelUtils.GetColumnHeaders(bytesFromExcel, "xlsx");
            var excelRows = ExcelUtils.GetTabularData(bytesFromExcel, "xlsx");
            var tableDataMS = new StandardTableDataCM()
            {
                FirstRowHeaders = true,
                Table = ExcelUtils.CreateTableCellPayloadObjects(excelRows, columnHeaders),
            };

            var curActionDTO = new ActivityDTO()
            {
                ParentPlanNodeId =  UtilitiesTesting.Fixtures.FixtureData.TestParentPlanID()
            };

            using (var crateStorage = _crateManager.GetUpdatableStorage(curActionDTO))
            {
                crateStorage.Add(Data.Crates.Crate.FromContent("", tableDataMS));
            }

            var restfulServiceClient = new Mock<IRestfulServiceClient>();
            restfulServiceClient.Setup(r => r.GetAsync<PayloadDTO>(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Dictionary<string,string>>()))
                .Returns(Task.FromResult(FixtureData.CratePayloadDTOForSendEmailViaSendGridConfiguration));
            ObjectFactory.Configure(cfg => cfg.For<IRestfulServiceClient>().Use(restfulServiceClient.Object));


            var curActivityDO = AutoMapper.Mapper.Map<ActivityDO>(curActionDTO);
            var result = await new Load_Excel_File_v1().Run(curActivityDO, UtilitiesTesting.Fixtures.FixtureData.TestContainer_Id_1(), null);

            var payloadCrates = _crateManager.GetStorage(result).CratesOfType<StandardPayloadDataCM>();
            var payloadDataMS = payloadCrates.First().Content;

            Assert.IsNotNull(result.CrateStorage);
            Assert.IsNotNull(payloadCrates);
            Assert.IsNotNull(payloadDataMS);
            Assert.AreEqual(payloadDataMS.PayloadObjects.Count, 3);
            Assert.AreEqual(payloadDataMS.PayloadObjects[0].PayloadObject[0].Key, "FirstName");
            Assert.AreEqual(payloadDataMS.PayloadObjects[0].PayloadObject[0].Value, "Alex");
        }
    }
}
