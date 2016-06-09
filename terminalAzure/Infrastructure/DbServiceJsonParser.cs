using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json.Linq;
using TerminalSqlUtilities;

namespace terminalAzure.Infrastructure
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

        public string ExtractConnectionString(ActivityContext activityContext)
        {
            var controlsMS = activityContext.ActivityPayload.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                throw new ApplicationException("No controls crate found.");
            }

            var connectionStringControl = controlsMS.Controls.FirstOrDefault(x => x.Name == "connection_string");

            if (connectionStringControl == null)
            {
                throw new ApplicationException("No connection_string control found.");
            }

            return connectionStringControl.Value;
        }

        /// <summary>
        /// Extract single Table instance from JSON object.
        /// </summary>
        private Table ExtractTable(JObject table)
        {
            var tableRows = ExtractRows(table.Properties());

            // Create table instance and return.
            return new Table(new TableInfo(SchemaName, TableName), tableRows);
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