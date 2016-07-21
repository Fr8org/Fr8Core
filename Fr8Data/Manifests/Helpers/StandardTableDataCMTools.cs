using System.Collections.Generic;
using System.Linq;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Newtonsoft.Json.Linq;

namespace Fr8Data.Manifests.Helpers
{
    public class StandardTableDataCMTools
    {
        public static void AppendToStandardTableData(StandardTableDataCM dest, StandardTableDataCM source)
        {
            if (dest == null || dest.Table == null || dest.Table.Count == 0)
            {
                return;
            }

            if (source == null || source.Table == null || source.Table.Count == 0)
            {
                return;
            }

            var columnSet = new HashSet<string>(dest.GetHeaderRow().Row.Select(x => x.Cell.Key));

            for (var i = 1; i < source.Table.Count; ++i)
            {
                var row = new TableRowDTO();

                foreach (var cell in source.Table[i].Row)
                {
                    if (!columnSet.Contains(cell.Cell.Key))
                    {
                        continue;
                    }

                    row.Row.Add(
                        new TableCellDTO()
                        {
                            Cell = cell.Cell.Clone()
                        });
                }

                dest.Table.Add(row);
            }
        }

        public static StandardTableDataCM ExtractPayloadCrateDataToStandardTableData(Crate crate)
        {
            if (crate.ManifestType.Id == (int)MT.StandardTableData)
            {
                return crate.Get<StandardTableDataCM>();
            }
            if (crate.ManifestType.Id == (int)MT.FieldDescription)
            {
                var fields = crate.Get<FieldDescriptionsCM>();
                return new StandardTableDataCM
                {
                    FirstRowHeaders = true,
                    Table = new List<TableRowDTO>
                                   {
                                       //Keys of fields will become column headers
                                       new TableRowDTO { Row = fields.Fields.Select(x => new TableCellDTO { Cell = new FieldDTO(x.Key, x.Key) }).ToList() },
                                       new TableRowDTO { Row = fields.Fields.Select(x => new TableCellDTO { Cell = x }).ToList() }
                                   }
                };
            }
            var tableData = new StandardTableDataCM
            {
                FirstRowHeaders = true,
                Table = new List<TableRowDTO>()
            };
            var headerIsAdded = false;


            var item = CrateStorageSerializer.Default.ConvertToDto(crate);

            var token = JToken.Parse(item.Contents.ToString());

            var jObject = token as JObject;
            if (jObject != null)
            {
                //check if jObject has some JArray properties
                var arrayProperty = jObject.Properties().FirstOrDefault(x => x.Value is JArray);

                //check how StandardPayloadDataCM is structured
                if (arrayProperty != null)
                {
                    foreach (var arrayItem in arrayProperty.Value)
                    {
                        //arrayItem is PayloadObjectDTO which on has an List<FieldDTO>
                        var innerArrayProperty = ((JObject)arrayItem).Properties().FirstOrDefault(x => x.Value is JArray);
                        if (innerArrayProperty != null)
                        {
                            var headerRow = new TableRowDTO();
                            var dataRow = new TableRowDTO();

                            foreach (var innerArrayItem in innerArrayProperty.Value)
                            {
                                //try to parse the property as FieldDTO
                                if (innerArrayItem is JObject)
                                {
                                    var fieldObj = (JObject)innerArrayItem;
                                    if (fieldObj.Property("key") != null && fieldObj.Property("value") != null)
                                    {
                                        headerRow.Row.Add(new TableCellDTO
                                        {
                                            Cell = new FieldDTO(fieldObj["key"].ToString(), fieldObj["key"].ToString())
                                        });
                                        dataRow.Row.Add(new TableCellDTO
                                        {
                                            Cell =
                                                new FieldDTO(fieldObj["key"].ToString(), fieldObj["value"].ToString())
                                        });
                                    }
                                }
                            }

                            if (!headerIsAdded)
                            {
                                tableData.Table.Add(headerRow);
                                headerIsAdded = true;
                            }
                            tableData.Table.Add(dataRow);
                        }

                        // StandardFileListCM manifest has structure like this.
                        else
                        {
                            var headerRow = new TableRowDTO();
                            var dataRow = new TableRowDTO();

                            foreach (var property in ((JObject)arrayItem).Properties())
                            {
                                //try to parse the property as FieldDTO
                                if (property.Name != null && property.Value != null)
                                {
                                    headerRow.Row.Add(new TableCellDTO
                                    {
                                        Cell = new FieldDTO(property.Name, property.Name)
                                    });
                                    dataRow.Row.Add(new TableCellDTO
                                    {
                                        Cell =
                                            new FieldDTO(property.Name, property.Value.ToString())
                                    });
                                }
                            }
                            if (!headerIsAdded)
                            {
                                tableData.Table.Add(headerRow);
                                headerIsAdded = true;
                            }
                            tableData.Table.Add(dataRow);
                        }
                    }
                }
                else
                {
                    var headerRow = new TableRowDTO();
                    var dataRow = new TableRowDTO();

                    foreach (JProperty property in jObject.Properties())
                    {
                        //try to parse the property as FieldDTO
                        if (property.Value is JObject)
                        {
                            var fieldObj = (JObject)property.Value;
                            if (fieldObj.Property("key") != null && fieldObj.Property("value") != null)
                            {
                                headerRow.Row.Add(new TableCellDTO { Cell = new FieldDTO(fieldObj["key"].ToString(), fieldObj["key"].ToString()) });
                                dataRow.Row.Add(new TableCellDTO { Cell = new FieldDTO(fieldObj["key"].ToString(), fieldObj["value"].ToString()) });
                            }
                        }
                        else
                        {
                            headerRow.Row.Add(new TableCellDTO { Cell = new FieldDTO(property.Name, property.Name) });
                            dataRow.Row.Add(new TableCellDTO { Cell = new FieldDTO(property.Name, property.Value.ToString()) });
                        }
                    }
                    if (!headerIsAdded)
                    {
                        tableData.Table.Add(headerRow);
                        headerIsAdded = true;
                    }
                    tableData.Table.Add(dataRow);
                }
            }
            return tableData;
        }
    }
}
