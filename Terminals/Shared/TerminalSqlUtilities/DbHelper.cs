using System.Collections.Generic;
using System.Data;

namespace TerminalSqlUtilities
{
    public class DbHelper
    {
        public IEnumerable<Row> ExtractDataRows(IDataReader reader)
        {
            var fieldNames = new List<string>();
            for (var i = 0; i < reader.FieldCount; ++i)
            {
                fieldNames.Add(reader.GetName(i));
            }

            var rows = new List<Row>();
            while (reader.Read())
            {
                var values = new List<FieldValue>();

                for (var i = 0; i < reader.FieldCount; ++i)
                {
                    values.Add(new FieldValue(fieldNames[i], reader.GetValue(i)));
                }

                rows.Add(new Row(values));
            }

            return rows;
        }

        public Table ExtractDataTable(TableInfo tableInfo, IDataReader reader)
        {
            return new Table(tableInfo, ExtractDataRows(reader));
        }
    }
}
