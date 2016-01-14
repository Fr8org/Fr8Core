using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using terminalExcel.Infrastructure;

namespace terminalIntegrationTests
{
    public partial class TerminalIntegrationTests
    {
        [Test, Ignore]
        public async void TerminalExcel_CallExtractData_Execute()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var route = new RouteDO()
                {
                    Id = UtilitiesTesting.Fixtures.FixtureData.TestParentRouteID(),
                    Name = "1",
                    RouteState = RouteState.Active
                };

                uow.RouteRepository.Add(route);

                uow.ContainerRepository.Add(new ContainerDO()
                {
                    Id = UtilitiesTesting.Fixtures.FixtureData.TestContainer_Id_1(),
                    Route = route,
                    CrateStorage = _crateManager.EmptyStorageAsStr(),
                    ContainerState = ContainerState.Executing
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

            var curActionDTO = new ActionDTO()
            {
                ContainerId = UtilitiesTesting.Fixtures.FixtureData.TestContainer_Id_1(),
                ParentRouteNodeId =  UtilitiesTesting.Fixtures.FixtureData.TestParentRouteID()
            };

            using (var updater = _crateManager.UpdateStorage(curActionDTO))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("", tableDataMS));
            }

            var restfulServiceClient = new Mock<IRestfulServiceClient>();
            restfulServiceClient.Setup(r => r.GetAsync<PayloadDTO>(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Dictionary<string,string>>()))
                .Returns(Task.FromResult(FixtureData.CratePayloadDTOForSendEmailViaSendGridConfiguration));
            ObjectFactory.Configure(cfg => cfg.For<IRestfulServiceClient>().Use(restfulServiceClient.Object));


            var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);
            var result = await new Load_Excel_File_v1().Run(curActionDO, curActionDTO.ContainerId, null);

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
