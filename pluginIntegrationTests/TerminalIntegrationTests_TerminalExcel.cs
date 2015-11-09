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
using terminalExcel.Actions;
using terminalExcel.Fixtures;
using terminalExcel.Infrastructure;

namespace terminalIntegrationTests
{
    public partial class TerminalIntegrationTests
    {
        [Test]
        public async void TerminalExcel_CallExtractData_Execute()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var route = new RouteDO()
                {
                    Id = 1,
                    Name = "1",
                    RouteState = RouteState.Active
                };

                uow.RouteRepository.Add(route);

                uow.ContainerRepository.Add(new ContainerDO()
                {
                    Id = 1,
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
                ProcessId = 1,
                ParentRouteNodeId = 1,
            };

            using (var updater = _crateManager.UpdateStorage(curActionDTO))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("", tableDataMS));
            }

            var result = await new Load_Table_Data_v1().Run(curActionDTO);

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
