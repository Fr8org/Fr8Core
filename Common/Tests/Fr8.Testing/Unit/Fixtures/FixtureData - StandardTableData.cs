using System.Collections.Generic;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.DataTransferObjects;

using System.Linq;

namespace Fr8.Testing.Unit.Fixtures
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
                            new TableCellDTO() { Cell = new KeyValueDTO("name", "name")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("emailaddress", "emailaddress")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("companyname", "companyname")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("title", "title")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("leadsource", "leadsource")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("phone", "phone")  },
                        }
                    },
                    new TableRowDTO
                    {
                        Row = new List<TableCellDTO>()
                        {
                            new TableCellDTO() { Cell = new KeyValueDTO("name", name)  },
                            new TableCellDTO() { Cell = new KeyValueDTO("emailaddress", emailAddress)  },
                            new TableCellDTO() { Cell = new KeyValueDTO("companyname", "Clorox")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("title", "Mrs")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("leadsource", "Kiosk - Moscone")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("phone", "(415) 444-3234")  },
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
                            new TableCellDTO() { Cell = new KeyValueDTO("name", "name")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("emailaddress", "emailaddress")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("companyname", "companyname")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("title", "title")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("leadsource", "leadsource")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("phone", "phone")  },
                        }
                    },
                    new TableRowDTO
                    {
                        Row = new List<TableCellDTO>()
                        {
                            new TableCellDTO() { Cell = new KeyValueDTO("name", name)  },
                            new TableCellDTO() { Cell = new KeyValueDTO("emailaddress", emailAddress)  },
                            new TableCellDTO() { Cell = new KeyValueDTO("companyname", "Clorox")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("title", "Mrs")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("leadsource", "Kiosk - Moscone")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("phone", "(415) 444-3234")  },
                        }
                    },
                    new TableRowDTO
                    {
                        Row = new List<TableCellDTO>()
                        {
                            new TableCellDTO() { Cell = new KeyValueDTO("name", name)  },
                            new TableCellDTO() { Cell = new KeyValueDTO("emailaddress", emailAddress)  },
                            new TableCellDTO() { Cell = new KeyValueDTO("companyname", "Clorox")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("title", "Mrs")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("leadsource", "Kiosk - Moscone")  },
                            new TableCellDTO() { Cell = new KeyValueDTO("phone", "(415) 444-3234")  },
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
