using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;

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
            return CrateStorageSerializer.Default.ConvertToDto(crate);
        }

        public Crate FromDto(CrateDTO crate)
        {
            return CrateStorageSerializer.Default.ConvertFromDto(crate);
        }

        public CrateStorage FromDto(CrateStorageDTO crateStorage)
        {
            return CrateStorageSerializer.Default.ConvertFromDto(crateStorage);
        }

        public ICrateStorageUpdater UpdateStorage(Expression<Func<CrateStorageDTO>> storageAccessExpression)
        {
            return new CrateStorageStorageUpdater(storageAccessExpression);
        }

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
            return JsonConvert.SerializeObject(CrateStorageSerializer.Default.ConvertToDto(new CrateStorage()));
        }

        public Crate CreateAuthenticationCrate(string label, AuthenticationMode mode)
        {
            return Crate.FromContent(label, new StandardAuthenticationCM()
            {
                Mode = mode
            });
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
            return Crate.FromContent(label, new EventSubscriptionCM() {Subscriptions = subscriptions.ToList()});
        }

        
        public Crate CreateStandardEventReportCrate(string label, EventReportCM eventReport)
        {
            return Crate.FromContent(label, eventReport);
        }

        public Crate CreateStandardTableDataCrate(string label, bool firstRowHeaders, params TableRowDTO[] table)
        {
            return Crate.FromContent(label, new StandardTableDataCM() { Table = table.ToList(), FirstRowHeaders = firstRowHeaders });
        }


        public Crate CreatePayloadDataCrate(string payloadDataObjectType, string crateLabel, StandardTableDataCM tableDataMS)
        {
            return Crate.FromContent(crateLabel, TransformStandardTableDataToStandardPayloadDataExcel(payloadDataObjectType, tableDataMS));
        }

        private StandardPayloadDataCM TransformStandardTableDataToStandardPayloadDataExcel(string curObjectType, StandardTableDataCM tableDataMS)
        {
            var payloadDataMS = new StandardPayloadDataCM()
            {
                PayloadObjects = new List<PayloadObjectDTO>(),
                ObjectType = curObjectType,
            };

            // Rows containing column names
            var columnHeadersRowDTO = tableDataMS.Table[0];

            for (int i = 1; i < tableDataMS.Table.Count; ++i) // Since first row is headers; hence i starts from 1
            {
                var tableRowDTO = tableDataMS.Table[i];
                var fields = new List<FieldDTO>();
                for (int j = 0; j < tableRowDTO.Row.Count; ++j)
                {
                    var tableCellDTO = tableRowDTO.Row[j];
                    var listFieldDTO = new FieldDTO()
                    {
                        Key = columnHeadersRowDTO.Row[j].Cell.Value,
                        Value = tableCellDTO.Cell.Value,
                    };
                    fields.Add(listFieldDTO);
                }
                payloadDataMS.PayloadObjects.Add(new PayloadObjectDTO() { PayloadObject = fields, });
            }

            return payloadDataMS;
        }

    }
}
