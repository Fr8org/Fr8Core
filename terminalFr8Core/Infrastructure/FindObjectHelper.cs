using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Enums;
using TerminalBase.BaseClasses;

namespace terminalFr8Core.Infrastructure
{
    internal class FindObjectHelper
    {
        public async Task<Dictionary<string, DbType>> ExtractColumnTypes(
            BaseTerminalAction action, ActionDTO actionDTO)
        {
            var upstreamCrates = await action.GetCratesByDirection<StandardDesignTimeFieldsCM>(
                actionDTO.Id,
                GetCrateDirection.Upstream
            );

            if (upstreamCrates == null) { return null; }

            var columnTypesCrate = upstreamCrates
                .FirstOrDefault(x => x.Label == "Sql Column Types");

            if (columnTypesCrate == null) { return null; }

            var columnTypes = columnTypesCrate.Content;

            if (columnTypes == null) { return null; }

            var columnTypeFields = columnTypes.Fields;
            if (columnTypeFields == null) { return null; }

            var columnTypeMap = new Dictionary<string, DbType>();
            foreach (var columnType in columnTypeFields)
            {
                columnTypeMap.Add(columnType.Key, (DbType)Enum.Parse(typeof(DbType), columnType.Value));
            }

            return columnTypeMap;
        }

        public string ConvertValueToString(object value, DbType dbType)
        {
            return Convert.ToString(value);
        }
    }
}