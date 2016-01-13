using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using Data.States;
using System.Threading.Tasks;

namespace Hub.Managers
{
    public partial class CrateManager : ICrateManager
    {
        public CrateStorageDTO ToDto(CrateStorage storage)
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

        public CrateStorage FromDto(CrateStorageDTO crateStorage)
        {
            return CrateStorageSerializer.Default.ConvertFromDto(crateStorage);
        }
        /// <summary>
        /// Use this method to edit CrateStorage repersented byt CrateStorageDTO property of some class instance. This method will return IDisposable updater.
        /// On Dispose it will write changes to the property specified by the Expression. 
        /// </summary>
        /// <param name="storageAccessExpression"></param>
        /// <returns></returns>
        public ICrateStorageUpdater UpdateStorage(Expression<Func<CrateStorageDTO>> storageAccessExpression)
        {
            return new CrateStorageStorageUpdater(storageAccessExpression);
        }

        /// <summary>
        /// Use this method to edit CrateStorage represented by string property of some class instance. This method will return IDisposable updater.
        /// On Dispose it will write changes to the property specified by the Expression. 
        /// </summary>
        /// <param name="storageAccessExpression"></param>
        /// <returns></returns>
        public ICrateStorageUpdater UpdateStorage(Expression<Func<string>> storageAccessExpression)
        {
            return new CrateStorageStorageUpdater(storageAccessExpression);
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

        public string CrateStorageAsStr(CrateStorage storage)
        {
            return JsonConvert.SerializeObject(CrateStorageSerializer.Default.ConvertToDto(storage));
        }

        public Crate CreateAuthenticationCrate(string label, AuthenticationMode mode)
        {
            return Crate.FromContent(label, new StandardAuthenticationCM()
            {
                Mode = mode
            });
        }

        public void AddLogMessage(string label, List<LogItemDTO> logItemList, ContainerDO containerDO)
        {
            if (String.IsNullOrEmpty(label))
                throw new ArgumentException("Parameter Label is empty");

            if (logItemList == null)
                throw new ArgumentNullException("Parameter LogItemDTO list is null.");

            if (containerDO == null)
                throw new ArgumentNullException("Parameter ContainerDO is null.");

            var curManifestSchema = new StandardLoggingCM()
            {
                Item = logItemList
            };

            using (var updater = UpdateStorage(() => containerDO.CrateStorage))
            {
                updater.CrateStorage.Add(Crate.FromContent(label, curManifestSchema));
            }
        }

        public Crate<StandardDesignTimeFieldsCM> CreateDesignTimeFieldsCrate(string label, params FieldDTO[] fields)
        {
            return Crate<StandardDesignTimeFieldsCM>.FromContent(label, new StandardDesignTimeFieldsCM() { Fields = fields.ToList() });
        }

        public Crate<StandardConfigurationControlsCM> CreateStandardConfigurationControlsCrate(string label, params ControlDefinitionDTO[] controls)
        {
            return Crate<StandardConfigurationControlsCM>.FromContent(label, new StandardConfigurationControlsCM() { Controls = controls.ToList() });
        }

        public Crate CreateStandardEventSubscriptionsCrate(string label, params string[] subscriptions)
        {
            return Crate.FromContent(label, new EventSubscriptionCM() { Subscriptions = subscriptions.ToList() });
        }
        
        public Crate CreateStandardEventReportCrate(string label, EventReportCM eventReport)
        {
            return Crate.FromContent(label, eventReport);
        }

        public Crate CreateValidationErrorOverviewCrate(string label, string message, string action)
        {
            return Crate.FromContent(label, new ErrorMessageCM() {Message = message, CurrentAction = action});
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
                    var fields = new List<FieldDTO>();
                    int colNumber = (tableDataMS.FirstRowHeaders) ? columnHeadersRowDTO.Row.Count : tableRowDTO.Row.Count;
                    for (int j = 0; j < colNumber; ++j)
                    {
                        var tableCellDTO = tableRowDTO.Row[j];
                        var listFieldDTO = new FieldDTO()
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
                        (crateContentType as StandardPayloadDataCM).TryGetValue(findKey, true, out key);
                    else
                        throw new Exception("Manifest type GetFieldByKey implementation is missing");
                }
            }

            return key;
        }

        public OperationalStateCM GetOperationalState(PayloadDTO payloadDTO)
        {
            CrateStorage curCrateStorage = FromDto(payloadDTO.CrateStorage);
            OperationalStateCM curOperationalState = curCrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
            return curOperationalState;
        }
        //This method returns one crate of the specified Manifest Type from the payload
        public T GetByManifest<T>(PayloadDTO payloadDTO) where T : Manifest
        {
            CrateStorage curCrateStorage = FromDto(payloadDTO.CrateStorage);
            var curCrate = curCrateStorage.CratesOfType<T>().Single().Content;
            return curCrate;
        }
        public IEnumerable<FieldDTO> GetFields(IEnumerable<Crate> crates)
        {
            var fields = new List<FieldDTO>();

            foreach (var crate in crates)
            {
                //let's pass unknown manifests for now
                if (!crate.IsKnownManifest)
                {
                    continue;
                }

                fields.AddRange(FindFieldsRecursive(crate.Get()));
            }

            return fields;
        }

        private static IEnumerable<FieldDTO> FindFieldsRecursive(Object obj)
        {
            var fields = new List<FieldDTO>();
            if (obj is IEnumerable)
            {

                var objList = obj as IEnumerable;
                foreach (var element in objList)
                {
                    fields.AddRange(FindFieldsRecursive(element));
                }
                return fields;
            }

            var objType = obj.GetType();
            bool isPrimitiveType = objType.IsPrimitive || objType.IsValueType || (objType == typeof(string));

            if (!isPrimitiveType)
            {
                var field = obj as FieldDTO;
                if (field != null)
                {
                    return new List<FieldDTO> { field };
                }

                var objProperties = objType.GetProperties();
                var objFields = objType.GetFields();
                foreach (var prop in objProperties)
                {
                    fields.AddRange(FindFieldsRecursive(prop.GetValue(obj)));
                }

                foreach (var prop in objFields)
                {
                    fields.AddRange(FindFieldsRecursive(prop.GetValue(obj)));
                }
            }

            return fields;
        }

        public IEnumerable<string> GetLabelsByManifestType(IEnumerable<Crate> crates, string manifestType)
        {
            return crates.Where(c => c.ManifestType.Type == manifestType)
                    .GroupBy(c => c.Label)
                    .Select(c => c.Key).ToList();
        }
    }
}
