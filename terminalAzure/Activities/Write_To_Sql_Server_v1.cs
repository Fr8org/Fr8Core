using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using StructureMap;
using terminalAzure.Infrastructure;
using terminalAzure.Services;
using TerminalSqlUtilities;

namespace terminalAzure.Activities
{
    public class Write_To_Sql_Server_v1 : ExplicitTerminalActivity
    {
        private readonly IDbProvider _dbProvider;

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("7150a1e3-a32a-4a0b-a632-42529e5fd24d"),
            Name = "Write_To_Sql_Server",
            Label = "Write to Azure Sql Server",
            Version = "1",
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[] { ActivityCategories.Forward }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;


        public Write_To_Sql_Server_v1(ICrateManager crateManager, IDbProvider dbProvider) 
            : base(crateManager)
        {
            _dbProvider = dbProvider;
        }

        //If the user provides no Connection String value, provide an empty Connection String field for the user to populate
        public override Task Initialize()
        {
            Storage.Clear();
            CreateControls();
            return Task.FromResult(0);
        }

        //if the user provides a connection string, this action attempts to connect to the sql server and get its columns and tables
        public override Task FollowUp()
        {
            //Verify controls, make sure that TextBox with value exists
            ValidateControls();
            //In all followup calls, update data fields of the configuration store          
            try
            {
                var connStringField = ConfigurationControls.Controls.First();
                var dropDownControl = ConfigurationControls.Controls.OfType<DropDownList>().FirstOrDefault();

                dropDownControl.ListItems = GetTables();
                if (!string.IsNullOrEmpty(dropDownControl.Value))
                {
                    string identityColumn = null;
                    GetIdentityColumn(connStringField.Value, dropDownControl.Value, identity =>
                    {
                        identityColumn = identity;
                    });
                    var textSourceControls = ConfigurationControls.Controls.OfType<TextSource>();
                    foreach (var control in textSourceControls.ToList())
                    {
                            RemoveControl<TextSource>(control.Name);
                    }
                    var columns = GetTableColumns(dropDownControl.Value);

                    foreach (var column in columns)
                    {
                        if(identityColumn != column.ColumnName)
                        {
                            var textSource = UiBuilder.CreateSpecificOrUpstreamValueChooser(column.ColumnName, column.ColumnName);
                            
                            if (column.isNullable == false)
                            {
                                textSource.Required = true;
                            }
                            AddControl(textSource);
                        }
                    }
                }
                //var contentsList = GetFieldMappings();
                //Storage.RemoveByLabel("Sql Table Columns");
                //this needs to be updated to hold Crates instead of FieldDefinitionDTO
                // Storage.Add("Sql Table Columns", new KeyValueListCM(contentsList.Select(col => new KeyValueDTO { Key = col, Value = col })));
            }
            catch(Exception e)
            {
                AddErrorToControl();
            }

            return Task.FromResult(0);
        }

        private void CreateControls()
        {
            AddControls(new TextBox
            {
                Label = "SQL Connection String",
                Name = "connection_string",
                Required = true
            },
            new Button
            {
                Label = "Continue",
                Name = "Continue",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onClick", "requestConfig")
                }
            }, new DropDownList
            {
                Name = "Table",
                Label = "Table",
                Events = new List<ControlEvent> { ControlEvent.RequestConfig }
            });
        }

        //===============================================================================================
        //Specialized Methods (Only found in this Action class)

        private const string ProviderName = "System.Data.SqlClient";
        private const string FieldMappingQuery = @"SELECT CONCAT('[', r.TABLE_NAME, '].', r.COLUMN_NAME) as tblcols " +
                                                 @"FROM ( " +
                                                    @"SELECT DISTINCT tbls.TABLE_NAME, cols.COLUMN_NAME " +
                                                    @"FROM INFORMATION_SCHEMA.Tables tbls INNER JOIN INFORMATION_SCHEMA.COLUMNS cols ON tbls.TABLE_NAME = cols.TABLE_NAME " +
                                                 @") r " +
                                                 @"ORDER BY r.TABLE_NAME, r.COLUMN_NAME";


        //CONFIGURATION-Related Methods
        //-----------------------------------------

        public List<ListItem> GetTables()
        {
            var connStringField = ConfigurationControls.Controls.First();
            List<ListItem> tableMetaData = new List<ListItem>();
            GetAllTables(connStringField.Value, tables =>
            {
                foreach (var table in tables)
                {
                    tableMetaData.Add(new ListItem() { Key = table.TableName, Value = table.TableName });
                }
            });
            return tableMetaData;
        }

        public List<ColumnInfo> GetTableColumns(string tableName)
        {
            var connStringField = ConfigurationControls.Controls.First();
            var list = new List<ColumnInfo>();
            ListTableColumns(connStringField.Value, tableName, columns =>
            {
                list.AddRange(columns);
            });

            return list;
        }

        public void GetAllTables(string connectionString, Action<IEnumerable<TableInfo>> callback)
        {
            using (var conn = _dbProvider.CreateConnection(connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    var columns = _dbProvider.ListAllTables(tx);

                    if (callback != null)
                    {
                        callback.Invoke(columns);
                    }
                }
            }
        }

        public void GetIdentityColumn(string connectionString, string tableName, Action<string> callback)
        {
            using (var conn = _dbProvider.CreateConnection(connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    var column = _dbProvider.GetIdentityColumn(tx, tableName);

                    if (callback != null)
                    {
                        callback.Invoke(column);
                    }
                }
            }
        }

        private void ListTableColumns(string connectionString, string tableName, Action<IEnumerable<ColumnInfo>> callback)
        {
            using (var conn = _dbProvider.CreateConnection(connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    var columns = _dbProvider.ListTableColumns(tx, tableName);

                    if (callback != null)
                    {
                        callback.Invoke(columns);
                    }
                }
            }
        }

        //public List<string> GetFieldMappings()
        //{
        //    var connStringField = ConfigurationControls.Controls.First();
            
        //    return (List<string>)_dbProvider.ConnectToSql(connStringField.Value, (command) =>
        //    {
        //        command.CommandText = FieldMappingQuery;

        //        //The list to serialize
        //        List<string> tableMetaData = new List<string>();

        //        //Iterate through each entry from the const query
        //        using (IDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                tableMetaData.Add(reader["tblcols"].ToString());
        //            }
        //        }

        //        //Serialize and return
        //        return tableMetaData;
        //    });
        //}

        private void ValidateControls()
        {
            if (Storage.Count == 0)
            {
                throw new TerminalCodedException(TerminalErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }

            if (ConfigurationControls == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }

            var connStringField = ConfigurationControls.Controls.First();
            if (string.IsNullOrEmpty(connStringField?.Value))
            {
                throw new TerminalCodedException(TerminalErrorCode.SQL_SERVER_CONNECTION_STRING_MISSING);
            }
        }
        private void AddErrorToControl()
        {
            var connStringTextBox = GetControl<TextBox>("connection_string");
            connStringTextBox.Value = "Incorrect Connection String";
        }

        //EXECUTION-Related Methods
        //-----------------------------------------
        private WriteCommandArgs PrepareSQLWrite()
        {
            var parser = new DbServiceJsonParser();
            var curConnStringObject = parser.ExtractConnectionString(ActivityContext);
            var curSQLData = CreateTable();
            return new WriteCommandArgs(ProviderName, curConnStringObject, curSQLData);
        }

        //private IEnumerable<Table> ConvertProcessPayloadToSqlInputs()
        //{
        //    var mappedFieldsCrate = Payload.CratesOfType<StandardPayloadDataCM>().FirstOrDefault(x => x.Label == "MappedFields");

        //    if (mappedFieldsCrate == null)
        //    {
        //        throw new ApplicationException("No payload crate found with Label == MappedFields.");
        //    }

        //    var valuesCrate = Payload.CratesOfType<StandardPayloadDataCM>().FirstOrDefault(x => x.Label == "TableData");

        //    if (valuesCrate == null)
        //    {
        //        throw new ApplicationException("No payload crate found with Label == TableData");
        //    }
        //    return CreateTables(mappedFieldsCrate.Content.AllValues().ToList(), valuesCrate.Content.AllValues().ToList());
        //}

        //private IEnumerable<Table> CreateTables(List<KeyValueDTO> fields, List<KeyValueDTO> values)
        //{
        //    // Map tableName -> field -> value.
        //    var tableMap = new Dictionary<string, Dictionary<string, string>>();
        //    Dictionary<string, string> columnMap = new Dictionary<string, string>();

        //    foreach (var field in fields)
        //    {
        //        var tableTokens = field.Value.Split('.');
        //        if (tableTokens.Length != 2)
        //        {
        //            throw new ApplicationException($"Invalid column name {field.Value}");
        //        }

        //        var tableName = PrepareSqlName(tableTokens[0]);
        //        var columnName = PrepareSqlName(tableTokens[1]);
        //        var valueField = values.FirstOrDefault(x => x.Key == field.Key);

        //        string value = "";
        //        if (valueField != null)
        //        {
        //            value = valueField.Value;
        //        }

        //        // Insert fields into table
        //        columnMap[columnName] = value;
        //        // Insert table into table dictionary
        //        tableMap[tableName] = columnMap;
        //    }

        //    var tables = CreateTablesFromMap(tableMap);
        //    return tables;
        //}

        //private IEnumerable<Table> CreateTablesFromMap(
        //    Dictionary<string, Dictionary<string, string>> tableMap)
        //{
        //    var tables = new List<Table>();

        //    foreach (var tablePair in tableMap)
        //    {
        //        var values = new List<FieldValue>();

        //        foreach (var columnPair in tablePair.Value)
        //        {
        //            values.Add(new FieldValue(columnPair.Key, columnPair.Value));
        //        }

        //        // TODO: change "dbo" schema later.
        //        tables.Add(new Table(new TableInfo("dbo", tablePair.Key), new[] { new Row(values) }));
        //    }

        //    return tables;
        //}

        private IEnumerable<Table> CreateTable()
        {
            //var tables = new List<Table>();
            var dropDownControl = ConfigurationControls.Controls.OfType<DropDownList>().FirstOrDefault();
            var textSourceControls = ConfigurationControls.Controls.OfType<TextSource>();
            var values = new List<FieldValue>();
            foreach (var control in textSourceControls)
            {
                if(!string.IsNullOrEmpty(control.TextValue))
                {
                    values.Add(new FieldValue(control.Name, control.TextValue));
                }
            }
            TableInfo tinfo = null;
            var connStringField = ConfigurationControls.Controls.First();
            GetAllTables(connStringField.Value, tables =>
            {
                tinfo = tables.Where(t => t.TableName == dropDownControl.Value).FirstOrDefault();
            });
            var table = new Table(tinfo, new[] { new Row(values) });
            var tabls = new List<Table>();
            tabls.Add(table);
            return tabls;
        }

        //private string PrepareSqlName(string rawName)
        //{
        //    return rawName.Replace("[", "").Replace("]", "");
        //}

        public override Task Run()
        {
            var curCommandArgs = PrepareSQLWrite();
            var dbService = new DbService();
            dbService.WriteCommand(curCommandArgs);
            Success();
            return Task.FromResult(0);
        }
    }
}