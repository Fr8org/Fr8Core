using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Helpers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.Managers
{
    public partial class CrateManager : ICrateManager
    {
        public CrateStorageDTO ToDto(ICrateStorage storage)
        {
            return CrateStorageSerializer.Default.ConvertToDto(storage);
        }

        public CrateDTO ToDto(Crate crate)
        {
            return crate != null ? CrateStorageSerializer.Default.ConvertToDto(crate) : null;
        }

        public Crate FromDto(CrateDTO crate)
        {
            return crate != null ? CrateStorageSerializer.Default.ConvertFromDto(crate) : null;
        }

        public ICrateStorage FromDto(CrateStorageDTO crateStorage)
        {
            return CrateStorageSerializer.Default.ConvertFromDto(crateStorage);
        }
        /// <summary>
        /// Use this method to edit CrateStorage repersented byt CrateStorageDTO property of some class instance. This method will return IDisposable updater.
        /// On Dispose it will write changes to the property specified by the Expression. 
        /// </summary>
        /// <param name="storageAccessExpression"></param>
        /// <returns></returns>
        public IUpdatableCrateStorage UpdateStorage(Expression<Func<CrateStorageDTO>> storageAccessExpression)
        {
            return new CrateManager.UpdatableCrateStorageStorage(storageAccessExpression);
        }
       
        /// <summary>
        /// Use this method to edit CrateStorage represented by string property of some class instance. This method will return IDisposable updater.
        /// On Dispose it will write changes to the property specified by the Expression. 
        /// </summary>
        /// <param name="storageAccessExpression"></param>
        /// <returns></returns>
        public IUpdatableCrateStorage UpdateStorage(Expression<Func<string>> storageAccessExpression)
        {
            return new CrateManager.UpdatableCrateStorageStorage(storageAccessExpression);
        }

        public bool IsEmptyStorage(CrateStorageDTO rawStorage)
        {
            if (rawStorage == null)
            {
                return true;
            }

            return FromDto(rawStorage).Count == 0;
        }

        public string EmptyStorageAsStr()
        {
            return CrateStorageAsStr(new CrateStorage());
        }

        public string CrateStorageAsStr(ICrateStorage storage)
        {
            return JsonConvert.SerializeObject(CrateStorageSerializer.Default.ConvertToDto(storage));
        }

        public string CrateStorageAsStr(CrateStorageDTO storageDTO)
        {
            return JsonConvert.SerializeObject(storageDTO);
        }

        public Crate CreateAuthenticationCrate(string label, AuthenticationMode mode, bool revocation)
        {
            return Crate.FromContent(label, new StandardAuthenticationCM()
            {
                Mode = mode,
                Revocation = revocation
            });
        }

        public Crate<KeyValueListCM> CreateDesignTimeFieldsCrate(string label, params KeyValueDTO[] fields)
        {
            return Crate<KeyValueListCM>.FromContent(label, new KeyValueListCM() { Values = fields.ToList() });
        }

        public Crate<ManifestDescriptionCM> CreateManifestDescriptionCrate(string label, string name, string id)
        {
            return Crate<ManifestDescriptionCM>.FromContent(label, new ManifestDescriptionCM() { Name = name, Id = id });
        }
        
        public Crate<KeyValueListCM> CreateDesignTimeFieldsCrate(string label, List<KeyValueDTO> fields)
        {
            return Crate<KeyValueListCM>.FromContent(label, new KeyValueListCM() { Values = fields });
        }
        
        public Crate<StandardConfigurationControlsCM> CreateStandardConfigurationControlsCrate(string label, params ControlDefinitionDTO[] controls)
        {
            return Crate<StandardConfigurationControlsCM>.FromContent(label,  new StandardConfigurationControlsCM() { Controls = controls.ToList() }, AvailabilityType.Configuration);
        }

        public Crate CreateStandardEventSubscriptionsCrate(string label, string manufacturer, params string[] subscriptions )
        {
            return Crate.FromContent(label, new EventSubscriptionCM() { Subscriptions = subscriptions.ToList(), Manufacturer = manufacturer});
        }
        
        public Crate CreateStandardEventReportCrate(string label, EventReportCM eventReport)
        {
            return Crate.FromContent(label, eventReport);
        }

        public Crate CreateStandardTableDataCrate(string label, bool firstRowHeaders, params TableRowDTO[] table)
        {
            return Crate.FromContent(label, new StandardTableDataCM() { Table = table.ToList(), FirstRowHeaders = firstRowHeaders });
        }



        public Crate CreateOperationalStatusCrate(string label, OperationalStateCM operationalStatus)
        {
            return Crate.FromContent(label, operationalStatus);
        }
        
        public Crate CreatePayloadDataCrate(string payloadDataObjectType, string crateLabel, StandardTableDataCM tableDataMS)
        {
            return Crate.FromContent(crateLabel, TransformStandardTableDataToStandardPayloadData(payloadDataObjectType, tableDataMS));
        }

        public StandardPayloadDataCM TransformStandardTableDataToStandardPayloadData(string curObjectType, StandardTableDataCM tableDataMS)
        {
            var payloadDataMS = new StandardPayloadDataCM()
            {
                PayloadObjects = new List<PayloadObjectDTO>(),
                ObjectType = curObjectType,
            };

            int staringRow;
            TableRowDTO columnHeadersRowDTO = null;

            if (tableDataMS.FirstRowHeaders)
            {
                staringRow = 1;
                columnHeadersRowDTO = tableDataMS.Table[0];
            }
            else
                staringRow = 0;

            // Rows containing column names
            for (int i = staringRow; i < tableDataMS.Table.Count; ++i) // Since first row is headers; hence i starts from 1
            {
                try
                {
                    var tableRowDTO = tableDataMS.Table[i];
                    var fields = new List<KeyValueDTO>();
                    int colNumber = (tableDataMS.FirstRowHeaders) ? columnHeadersRowDTO.Row.Count : tableRowDTO.Row.Count;
                    for (int j = 0; j < colNumber; ++j)
                    {
                        var tableCellDTO = tableRowDTO.Row[j];
                        var listFieldDTO = new KeyValueDTO()
                        {
                            Key = (tableDataMS.FirstRowHeaders) ? columnHeadersRowDTO.Row[j].Cell.Value : tableCellDTO.Cell.Key,
                            Value = tableCellDTO.Cell.Value
                        };
                        fields.Add(listFieldDTO);
                    }
                    payloadDataMS.PayloadObjects.Add(new PayloadObjectDTO() { PayloadObject = fields });
                }
                catch (Exception)
                {
                    //Avoid general failure of the process if there is an error processing individual records in the table
                }
            }

            return payloadDataMS;
        }


        public string GetFieldByKey<T>(CrateStorageDTO curCrateStorage, string findKey) where T : Manifest
        {
            string key = string.Empty;
            
            if (curCrateStorage != null)
            {
                var crateStorage = this.FromDto(curCrateStorage);
                var crateContentType = crateStorage.CrateContentsOfType<T>().FirstOrDefault();

                if (crateContentType != null)
                {
                    if (crateContentType is StandardPayloadDataCM)
                        (crateContentType as StandardPayloadDataCM).TryGetValue(findKey, true, false, out key);
                    else
                        throw new Exception("Manifest type GetFieldByKey implementation is missing");
                }
            }

            return key;
        }

        public OperationalStateCM GetOperationalState(ICrateStorage crateStorage)
        {
            OperationalStateCM curOperationalState = crateStorage.CrateContentsOfType<OperationalStateCM>().Single();
            return curOperationalState;
        }
        //This method returns one crate of the specified Manifest Type from the payload
        public T GetByManifest<T>(PayloadDTO payloadDTO) where T : Manifest
        {
            ICrateStorage curCrateStorage = FromDto(payloadDTO.CrateStorage);
            var curCrate = curCrateStorage.CratesOfType<T>().Single().Content;
            return curCrate;
        }
        public IEnumerable<KeyValueDTO> GetFields(IEnumerable<Crate> crates)
        {
            var fields = new List<KeyValueDTO>();

            foreach (var crate in crates)
            {
                //let's pass unknown manifests for now
                if (!crate.IsKnownManifest)
                {
                    continue;
                }

                fields.AddRange(Fr8ReflectionHelper.FindFieldsRecursive(crate.Get()));
            }

            return fields;
        }

        public IEnumerable<string> GetLabelsByManifestType(IEnumerable<Crate> crates, string manifestType)
        {
            return crates.Where(c => c.ManifestType.Type == manifestType)
                    .GroupBy(c => c.Label)
                    .Select(c => c.Key).ToList();
        }

        public T GetContentType<T>(string crate) where T : class
        {
            return this.GetStorage(crate)
                            .CrateContentsOfType<T>()
                            .FirstOrDefault();
        }
    }
}
