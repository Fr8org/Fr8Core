using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    public class StandardTableDataCM : Manifest
    {
        public StandardTableDataCM()
			  :base(Constants.MT.StandardTableData)
        {
            Table = new List<TableRowDTO>();
        }

        public List<TableRowDTO> Table { get; set; }
        public bool FirstRowHeaders { get; set; }

        public TableRowDTO GetHeaderRow()
        {
            return Table[0];
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

    public class TableCellDTO
    {
        public TableCellDTO()
        {
            FieldDTO Cell = new FieldDTO();  //name should be the column number, and value is the value of the cell           
        }

        public FieldDTO Cell;

        public static TableCellDTO Create(string key, string value)
        {
            return new TableCellDTO()
            {
                Cell = new FieldDTO()
                {
                    Key = key,
                    Value = value,
                }
            };
        }
    }
}
