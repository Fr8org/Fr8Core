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
using Data.Interfaces.ManifestSchemas;
using Data.States;
using Hub.Managers;
using terminalExcel.Actions;
using terminalExcel.Fixtures;
using terminalExcel.Infrastructure;

namespace pluginIntegrationTests
{
    public partial class PluginIntegrationTests
    {
        [Test]
        public async void PluginExcel_CallExtractData_Execute()
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
                    CrateStorage = JsonConvert.SerializeObject(new PayloadDTO("", 0)),
                    ContainerState = ContainerState.Executing
                });

                uow.SaveChanges();
            }
            
            var crate = new CrateManager();

            var bytesFromExcel = PluginFixtureData.TestExcelData();
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
                CrateStorage = new CrateStorageDTO()
                {
                    CrateDTO = new System.Collections.Generic.List<CrateDTO>() 
                    { 
                        new CrateDTO()
                        {
                            ManifestId = CrateManifests.STANDARD_TABLE_DATA_MANIFEST_ID,
                            ManifestType = CrateManifests.STANDARD_TABLE_DATA_MANIFEST_NAME,
                            Contents = JsonConvert.SerializeObject(tableDataMS),
                        },
                    },
                },
            };

            var result = await new Load_Table_Data_v1().Run(curActionDTO);
            var payloadCrates = crate.GetCratesByManifestType(CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME, result.CrateStorageDTO());
            var payloadDataMS = JsonConvert.DeserializeObject<StandardPayloadDataCM>(payloadCrates.First().Contents);

            Assert.IsNotNull(result.CrateStorage);
            Assert.IsNotNull(payloadCrates);
            Assert.IsNotNull(payloadDataMS);
            Assert.AreEqual(payloadDataMS.PayloadObjects.Count, 3);
            Assert.AreEqual(payloadDataMS.PayloadObjects[0].PayloadObject[0].Key, "FirstName");
            Assert.AreEqual(payloadDataMS.PayloadObjects[0].PayloadObject[0].Value, "Alex");
        }
    }
}
