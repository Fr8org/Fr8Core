using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

namespace pluginAzureSqlServer.Infrastructure
{
    public class JsonExtractionHelper
    {
        /// <summary>
        /// Extract WriteCommandArgs instance from raw JSON data.
        /// </summary>
        /// <param name="data">Raw JSON data.</param>
        public WriteCommandArgs ExtractWriteCommandArgs(JObject data)
        {
            // This is a sample json data we're trying to parse.
            // {
	        //     "connectionString": "Data Source=.;Initial Catalog=TestDB;",
	        //     "provider": "System.Data.SqlClient",
	        //     "tables": [ 
		    //         {
			//             "Customers": [
			// 	               {
			// 		               "firstName": "John",
			// 		               "lastName": "Smith"
			// 	               },
			// 	               {
			// 		               "firstName": "Sam", 
			// 		               "lastName": "Jones"
			// 	               },
			//             ]
		    //         }
	        //     ]
            // }

            var provider = ExctractProviderName(data);
            var connectionString = ExtractConnectionString(data);
            var tables = ExtractTables(data);

            var writeArgs = new WriteCommandArgs(provider, connectionString, tables);

            return writeArgs;
        }

        /// <summary>
        /// Extract providerName from JSON object.
        /// </summary>
        private string ExctractProviderName(JObject data)
        {
            var providerToken = data.GetValue("provider");
            if (providerToken == null
                || string.IsNullOrEmpty(providerToken.ToObject<string>()))
            {
                throw new Exception("\"provider\" attribute is not specified");
            }

            var provider = providerToken.ToObject<string>();

            return provider;
        }

        /// <summary>
        /// Extract connectionString from JSON object.
        /// </summary>
        private string ExtractConnectionString(JObject data)
        {
            var connectionStringToken = data.GetValue("connectionString");
            if (connectionStringToken == null
                || string.IsNullOrEmpty(connectionStringToken.ToObject<string>()))
            {
                throw new Exception("\"connectionsString\" attribute is not specified");
            }

            var connectionString = connectionStringToken.ToObject<string>();

            return connectionString;
        }

        /// <summary>
        /// Extract tables array from JSON object.
        /// </summary>
        private IEnumerable<Table> ExtractTables(JObject data)
        {
            // Try to get "tables" property as js array.
            // Validate that "tables" array is not empty.
            var tablesToken = data.GetValue("tables");
            if (tablesToken == null
                || tablesToken.ToObject<JArray>() == null
                || tablesToken.ToObject<JArray>().Count == 0)
            {
                throw new Exception("\"tables\" array is not specified");
            }

            var tablesArray = tablesToken.ToObject<JArray>();
            var tables = new List<Table>();

            // Iterate "tables" array, extract each table and put it into result list.
            foreach (var tableToken in tablesArray)
            {
                var table = ExtractTable(tableToken);
                // If table is empty (i.e. contains no properties), 
                // then we assume that table definition is corrupted and we skip it.
                if (table == null) { continue; }

                tables.Add(table);
            }

            return tables;
        }

        /// <summary>
        /// Extract single Table instance from JSON object.
        /// </summary>
        private Table ExtractTable(JToken tableToken)
        {
            var tableObj = tableToken.ToObject<JObject>();

            // Get the first property of table object, first property will be the name of the table (see JSON sample above).
            var prop = tableObj.Properties().FirstOrDefault();
            // If table object contains no properties, then return null.
            if (prop == null) { return null; }

            // Full table name formatted as <schemaName>.<tableName> or <tableName>
            var fullTableName = prop.Name;
            // Split full table name by dot char.
            var tableNameTokens = fullTableName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            string schemaName = null;
            string tableName;
            // In case if full table name contains schema and table names.
            if (tableNameTokens.Length > 1)
            {
                schemaName = tableNameTokens[0];
                tableName = tableNameTokens[1];
            }
            // In case if full table name contains only table name.
            else
            {
                tableName = tableNameTokens[0];
            }

            // Extract rows from table JSON definition.
            var rowsArray = tableObj.GetValue(fullTableName).ToObject<JArray>();
            var rows = ExtractRows(rowsArray);

            // Create table instance and return.
            var table = new Table(schemaName, tableName, rows);
            return table;
        }

        /// <summary>
        /// Extract rows for single table.
        /// </summary>
        private IEnumerable<Row> ExtractRows(JArray rowsArray)
        {
            var rows = new List<Row>();

            // Iterate rows array, extract each row and add it to result list.
            foreach (var rowToken in rowsArray)
            {
                var row = ExtractRow(rowToken);
                rows.Add(row);
            }

            return rows;
        }

        /// <summary>
        /// Extract single row from JSON object.
        /// </summary>
        private Row ExtractRow(JToken rowToken)
        {
            var rowObj = rowToken.ToObject<JObject>();

            // Field / value list.
            var values = new List<FieldValue>();

            // Iterate properties of row object.
            foreach (var fieldName in rowObj.Properties())
            {
                // Get field value.
                var value = rowObj.GetValue(fieldName.Name).ToObject<object>();

                // Add field / value pair to result list.
                values.Add(new FieldValue(fieldName.Name, value));
            }

            // Create row instance and return it.
            var row = new Row(values);
            return row;
        }
    }
}