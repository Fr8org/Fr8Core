using System.Collections.Generic;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using System.Linq;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static StandardTableDataCM TestStandardTableData(string emailAddress, string name)
        {
            return new StandardTableDataCM()
            {
                FirstRowHeaders = true,
                Table = new List<TableRowDTO>()
                {
                    new TableRowDTO
                    {
                        Row = new List<TableCellDTO>()
                        {
                            new TableCellDTO() { Cell = new FieldDTO("name", "name")  },
                            new TableCellDTO() { Cell = new FieldDTO("emailaddress", "emailaddress")  },
                            new TableCellDTO() { Cell = new FieldDTO("companyname", "companyname")  },
                            new TableCellDTO() { Cell = new FieldDTO("title", "title")  },
                            new TableCellDTO() { Cell = new FieldDTO("leadsource", "leadsource")  },
                            new TableCellDTO() { Cell = new FieldDTO("phone", "phone")  },
                        }
                    },
                    new TableRowDTO
                    {
                        Row = new List<TableCellDTO>()
                        {
                            new TableCellDTO() { Cell = new FieldDTO("name", name)  },
                            new TableCellDTO() { Cell = new FieldDTO("emailaddress", emailAddress)  },
                            new TableCellDTO() { Cell = new FieldDTO("companyname", "Clorox")  },
                            new TableCellDTO() { Cell = new FieldDTO("title", "Mrs")  },
                            new TableCellDTO() { Cell = new FieldDTO("leadsource", "Kiosk - Moscone")  },
                            new TableCellDTO() { Cell = new FieldDTO("phone", "(415) 444-3234")  },
                        }
                    }
                }
            };
        }

        public static StandardTableDataCM TestStandardTableData()
        {
            return TestStandardTableData("test@fr8.co", "samplename");
        }

        public static StandardTableDataCM TestStandardTableData_NoHeader()
        {
            var table = TestStandardTableData();
            table.Table.RemoveAt(0);
            return table;
        }

        public static StandardTableDataCM TestStandardTableData_TwoRowsNoHeader()
        {
            var table = TestStandardTableData_TwoRows();
            table.Table.RemoveAt(0);
            return table;
        }

        public static StandardTableDataCM TestStandardTableData_TwoRows(string emailAddress, string name)
        {
            return new StandardTableDataCM()
            {
                FirstRowHeaders = true,
                Table = new List<TableRowDTO>()
                {
                    new TableRowDTO
                    {
                        Row = new List<TableCellDTO>()
                        {
                            new TableCellDTO() { Cell = new FieldDTO("name", "name")  },
                            new TableCellDTO() { Cell = new FieldDTO("emailaddress", "emailaddress")  },
                            new TableCellDTO() { Cell = new FieldDTO("companyname", "companyname")  },
                            new TableCellDTO() { Cell = new FieldDTO("title", "title")  },
                            new TableCellDTO() { Cell = new FieldDTO("leadsource", "leadsource")  },
                            new TableCellDTO() { Cell = new FieldDTO("phone", "phone")  },
                        }
                    },
                    new TableRowDTO
                    {
                        Row = new List<TableCellDTO>()
                        {
                            new TableCellDTO() { Cell = new FieldDTO("name", name)  },
                            new TableCellDTO() { Cell = new FieldDTO("emailaddress", emailAddress)  },
                            new TableCellDTO() { Cell = new FieldDTO("companyname", "Clorox")  },
                            new TableCellDTO() { Cell = new FieldDTO("title", "Mrs")  },
                            new TableCellDTO() { Cell = new FieldDTO("leadsource", "Kiosk - Moscone")  },
                            new TableCellDTO() { Cell = new FieldDTO("phone", "(415) 444-3234")  },
                        }
                    },
                    new TableRowDTO
                    {
                        Row = new List<TableCellDTO>()
                        {
                            new TableCellDTO() { Cell = new FieldDTO("name", name)  },
                            new TableCellDTO() { Cell = new FieldDTO("emailaddress", emailAddress)  },
                            new TableCellDTO() { Cell = new FieldDTO("companyname", "Clorox")  },
                            new TableCellDTO() { Cell = new FieldDTO("title", "Mrs")  },
                            new TableCellDTO() { Cell = new FieldDTO("leadsource", "Kiosk - Moscone")  },
                            new TableCellDTO() { Cell = new FieldDTO("phone", "(415) 444-3234")  },
                        }
                    }
                }
            };
        }

        public static StandardTableDataCM TestStandardTableData_TwoRows()
        {
            return TestStandardTableData_TwoRows("test@fr8.co", "samplename");
        }

        public static string[] TestStandardTableData_HeadersOnly()
        {
            return TestStandardTableData().Table[0].Row.Select(c => c.Cell.Value).ToArray();
        }
    }
}
