using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class StandardTableDataCM : Manifest
    {
        public List<TableRowDTO> Table { get; set; }

        [ManifestField(IsHidden = true)]
        public bool FirstRowHeaders { get; set; }

        [ManifestField(IsHidden = true)]
        public bool HasDataRows
        {
            get
            {
                if (Table?.Count == 0)
                {
                    return false;
                }
                return Table.Count > 1 || (!FirstRowHeaders && Table.Count == 1);
            }
        }

        public IEnumerable<TableRowDTO> DataRows
        {
            get
            {
                if (Table == null)
                {
                    return new TableRowDTO[0];
                }
                return Table.Skip(FirstRowHeaders ? 1 : 0);
            }
        }

        public StandardTableDataCM(bool isFirstRowHeaders, IEnumerable<TableRowDTO> rows)
                          : base(MT.StandardTableData)
        {
            FirstRowHeaders = isFirstRowHeaders;
            Table = new List<TableRowDTO>(rows);
        }
        
        public StandardTableDataCM()
              : base(MT.StandardTableData)
        {
            Table = new List<TableRowDTO>();
        }

        public TableRowDTO GetHeaderRow()
        {
            return Table[0];
        }

        public StandardPayloadDataCM ToPayloadData()
        {
            var result = new StandardPayloadDataCM();

            var payloadObjects = DataRows
                .Select(x => new PayloadObjectDTO()
                {
                    PayloadObject = x.Row.Select(y => y.Cell).ToList()
                })
                .ToList();

            result.PayloadObjects = payloadObjects;

            return result;
        }
    }

    public class TableRowDTO
    {
        public TableRowDTO()
        {
            Row = new List<TableCellDTO>();
        }

        public List<TableCellDTO> Row;
    }

    [System.Diagnostics.DebuggerDisplay("Key = '{Cell.Key}', Value = '{Cell.Value}'")]
    public class TableCellDTO
    {
        public TableCellDTO()
        {
            KeyValueDTO Cell = new KeyValueDTO();  //name should be the column number, and value is the value of the cell           
        }

        public KeyValueDTO Cell;

        public static TableCellDTO Create(string key, string value)
        {
            return new TableCellDTO()
            {
                Cell = new KeyValueDTO()
                {
                    Key = key,
                    Value = value,
                }
            };
        }
    }
}
