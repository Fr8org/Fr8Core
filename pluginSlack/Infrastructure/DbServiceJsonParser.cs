using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginBase.Infrastructure;

namespace pluginSlack.Infrastructure
{
    public class DbServiceJsonParser
    {
        //since we're working with a single table, leaving these constants for now.
        private const string TableName = "Customer";
        private const string SchemaName = "dbo";

        /// <summary>
        /// Create Table instance from raw JSON data.
        /// </summary>
        /// <param name="data">Raw JSON data.</param>
        public Table CreateTable(JObject data)
        {
            // This is a sample json data we're trying to parse.
            // {
     	    //          {
     		//             "firstName": "John",
     		//             "lastName": "Smith"
     	    //          }
            // }

            return ExtractTable(data);
        }

        public string ExtractConnectionString(ActionDO curActionDO)
        {
            var curConnectionString = string.Empty;
            var curSettings = JsonConvert.DeserializeObject<JObject>(curActionDO.CrateStorage);
            var curConnStringObject = curSettings.ExtractPropertyValue<JArray>("configurationSettings");

            foreach (var v in curConnStringObject.Select(item => item.SelectToken("textField")).Where(v => v != null))
            {
                curConnectionString = v.SelectToken("value").ToString();
                break;
            }

            return curConnectionString;
        }

        /// <summary>
        /// Extract single Table instance from JSON object.
        /// </summary>
        private Table ExtractTable(JObject table)
        {
            var tableRows = ExtractRows(table.Properties());

            // Create table instance and return.
            return new Table(SchemaName, TableName, tableRows);
        }

        /// <summary>
        /// Extract rows for single table.
        /// Single row for now.
        /// </summary>
        private IEnumerable<Row> ExtractRows(IEnumerable<JProperty> properties)
        {
            var values = properties.Select(property => new FieldValue(property.Name, property.Value)).ToList();
            return new List<Row> {new Row(values)};
        }
    }
}