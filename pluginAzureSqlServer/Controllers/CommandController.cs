using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using StructureMap;
using Core.Plugins.AzureSql;
using pluginAzureSqlServer.Messages;

namespace pluginAzureSqlServer.Controllers
{
    public class CommandController : ApiController
    {
        [HttpPost]
        [Route("writeSQL")]
        public CommandResponse Write(JObject data)
        {
            /*
            {
	            "connectionString": "Data Source=.;Initial Catalog=TestDB;",
	            "provider": "System.Data.SqlClient",
	            "tables": [ 
		            {
			            "Customers": [
				            {
					            "firstName": "John",
					            "lastName": "Smith"
				            },
				            {
					            "firstName": "Sam", 
					            "lastName": "Jones"
				            },
			            ]
		            }
	            ]
            }
            */

            var providerToken = data.GetValue("provider");
            if (providerToken == null
                || string.IsNullOrEmpty(providerToken.ToObject<string>()))
            {
                return CommandResponse.ErrorResponse("\"provider\" attribute is not specified");
            }

            var connectionStringToken = data.GetValue("connectionString");
            if (connectionStringToken == null
                || string.IsNullOrEmpty(connectionStringToken.ToObject<string>()))
            {
                return CommandResponse.ErrorResponse("\"connectionsString\" attribute is not specified");
            }

            var tablesToken = data.GetValue("tables");
            if (tablesToken == null
                || tablesToken.ToObject<JArray>() == null
                || tablesToken.ToObject<JArray>().Count == 0)
            {
                return CommandResponse.ErrorResponse("\"tables\" array is not specified");
            }

            var provider = providerToken.ToObject<string>();
            var connectionString = connectionStringToken.ToObject<string>();

            var tables = tablesToken.ToObject<JArray>();
            var resultTables = new List<Table>();

            for (var i = 0; i < tables.Count; ++i)
            {
                var tableObj = tables[i].ToObject<JObject>();
                
                var prop = tableObj.Properties().FirstOrDefault();
                if (prop == null) { continue; }

                var fullTableName = prop.Name;
                var tableNameTokens = fullTableName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                
                string schema = null;
                string table;
                if (tableNameTokens.Length > 1)
                {
                    schema = tableNameTokens[0];
                    table = tableNameTokens[1];
                }
                else
                {
                    table = tableNameTokens[0];
                }


                var rows = tableObj.GetValue(fullTableName).ToObject<JArray>();
                var resultRows = new List<Row>();

                for (var j = 0; j < rows.Count; ++j)
                {
                    var row = rows[j].ToObject<JObject>();

                    var resultValues = new List<FieldValue>();
                    foreach (var fieldName in row.Properties())
                    {
                        var value = row.GetValue(fieldName.Name).ToObject<object>();
                        resultValues.Add(new FieldValue(fieldName.Name, value));
                    }

                    resultRows.Add(new Row(resultValues));
                }

                resultTables.Add(new Table(schema, table, resultRows));
            }

            var writeArgs = new WriteCommandArgs(provider, connectionString, resultTables);

            var azureSqlPlugin = ObjectFactory.GetInstance<IAzureSqlPlugin>();
            try
            {
                azureSqlPlugin.WriteCommand(writeArgs);
            }
            catch (Exception ex)
            {
                return CommandResponse.ErrorResponse(ex.Message);
            }

            return CommandResponse.SuccessResponse();
        }
    }
}
