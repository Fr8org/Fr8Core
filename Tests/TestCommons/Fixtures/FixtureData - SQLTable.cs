using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCommons.Fixtures
{
    partial class FixtureData
    {
        public string TestCustomerTable1_Schema()
        {
            return "dbo";
        }

        public string TestCustomerTable1_Table()
        {
            return "Customers";
        }

        public string TestCustomerTable1_Create()
        {
            return @"CREATE TABLE [dbo].[Customers] (
                        [FirstName] NVARCHAR(100) NOT NULL,
                        [LastName] NVARCHAR(100) NOT NULL,
                        PRIMARY KEY CLUSTERED ([FirstName] ASC, [LastName] ASC)
                    )";
        }

        public string TestCustomerTable1_Drop()
        {
            return @"DROP TABLE [dbo].[Customers]";
        }

        public string TestCustomerTable1_Json()
        {
            return @"""Customers"": [
			    {
			    ""firstName"": ""John"",
			    ""lastName"": ""Smith""
			    },
			    {
			    ""firstName"": ""Sam"", 
			    ""lastName"": ""Jones""
			    },
			]";
        }
    }
}
