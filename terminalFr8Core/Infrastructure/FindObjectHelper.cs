using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Data.Entities;
using Newtonsoft.Json;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;

using TerminalBase.BaseClasses;
using Utilities.Configuration.Azure;

namespace terminalFr8Core.Infrastructure
{
    internal class FindObjectHelper
    {
        public async Task<Dictionary<string, DbType>> ExtractColumnTypes(
            BaseTerminalAction action, ActionDO actionDO)
        {
            var upstreamCrates = await action.GetCratesByDirection<StandardDesignTimeFieldsCM>(
                actionDO,
                CrateDirection.Upstream
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

        public async Task LaunchContainer(int routeId)
        {
            var httpClient = new HttpClient();
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "/routes/run?routeId=" + routeId.ToString();

            using (var response = await httpClient.GetAsync(url).ConfigureAwait(false))
            {
                await response.Content.ReadAsStringAsync();
            }
        }
    }
}