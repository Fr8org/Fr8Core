using System;
using System.Collections.Generic;
using Data.Entities;
using System.Globalization;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;

namespace terminalQuickBooksTests.Fixtures
{
    class HealthMonitor_FixtureData
    {
        public static AuthorizationTokenDTO QuickBooks_AuthTokenDTO()
        {
            return new AuthorizationTokenDTO()
            {
                Token = "qyprdCEtrUuHBA7TbmvyGJyHNTTSXF1mG028g3Ld3oRvb7lu;;;;;;;f1pHxtkM325nRAufGL7KCZWxW3Rlfp8t2ow5DrFx;;;;;;;1429888620;;;;;;;12/12/2016 00:00:00"
            };
        }

        public static AuthorizationTokenDO QuickBooks_AuthTokenDO()
        {
            return new AuthorizationTokenDO()
            {
                Token = "qyprdCEtrUuHBA7TbmvyGJyHNTTSXF1mG028g3Ld3oRvb7lu;;;;;;;f1pHxtkM325nRAufGL7KCZWxW3Rlfp8t2ow5DrFx;;;;;;;1429888620"
            };
        }

        public static ActivityTemplateSummaryDTO Activity_Create_Journal_Entry_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Create_Journal_Entry_TEST",
                Version = "1"
            };
        }
        public static Fr8DataDTO Activity_Create_Journal_Entry_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Activity_Create_Journal_Entry_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Create Journal Entry",
                AuthToken = QuickBooks_AuthTokenDTO(),
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
        public static ActivityTemplateSummaryDTO Convert_TableData_To_AccountingTransactions_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Convert_TableData_To_AccountingTransactions_TEST",
                Version = "1"
            };
        }
        public static Fr8DataDTO Convert_TableData_To_AccountingTransactions_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Convert_TableData_To_AccountingTransactions_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Convert TableData To AccountingTransactions",
                AuthToken = QuickBooks_AuthTokenDTO(),
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
        public static StandardAccountingTransactionCM GetAccountingTransactionCM()
        {
            var curFinLineDTOList = new List<FinancialLineDTO>();
            var curFirstLineDTO = new FinancialLineDTO()
            {
                Amount = "100",
                AccountId = "1",
                AccountName = "Account-A",
                DebitOrCredit = "Debit",
                Description = "Move money to Accout-B"
            };
            var curSecondLineDTO = new FinancialLineDTO()
            {
                Amount = "100",
                AccountId = "2",
                AccountName = "Account-B",
                DebitOrCredit = "Credit",
                Description = "Move money from Accout-A"
            };
            curFinLineDTOList.Add(curFirstLineDTO);
            curFinLineDTOList.Add(curSecondLineDTO);


            var curAccoutingTransactionDTO = new StandardAccountingTransactionDTO()
            {
                Memo = "That is the test crate",
                FinancialLines = curFinLineDTOList,
                Name = "Code1",
                TransactionDate = DateTime.Parse("15-Dec-2015", new CultureInfo("en-US"))
            };
            var curCrate = new StandardAccountingTransactionCM()
            {
                AccountingTransactions = new List<StandardAccountingTransactionDTO>()
                {
                   curAccoutingTransactionDTO
                }
            };
            return curCrate;
        }

        public static StandardTableDataCM StandardTableData_Test1()
        {
            var headerRow = new TableRowDTO()
            {
                Row = new List<TableCellDTO>()
                {
                    new TableCellDTO()
                    {
                        Cell = new KeyValueDTO("1", "Date")
                    },
                    new TableCellDTO()
                    {
                        Cell = new KeyValueDTO("2", "Description")
                    },
                    new TableCellDTO()
                    {
                        Cell = new KeyValueDTO("3", "Phone")
                    },
                    new TableCellDTO()
                    {
                        Cell = new KeyValueDTO("4", "Travelling")
                    }
                }
            };
            var dataRow1 = new TableRowDTO()
            {
                Row = new List<TableCellDTO>()
                {
                    new TableCellDTO()
                    {
                        Cell = new KeyValueDTO("5", "30-Dec-2015")
                    },
                    new TableCellDTO()
                    {
                        Cell = new KeyValueDTO("6", "Trip to Samarkand")
                    },
                    new TableCellDTO()
                    {
                        Cell = new KeyValueDTO("7", "70")
                    },
                    new TableCellDTO()
                    {
                        Cell = new KeyValueDTO("8", "90")
                    }
                }
            };
            return new StandardTableDataCM
            {
                FirstRowHeaders = true,
                Table = new List<TableRowDTO> {headerRow, dataRow1}
            };
        }

        public static ChartOfAccountsCM ChartOfAccounts_Test1()
        {
            return new ChartOfAccountsCM
            {
                Accounts = new List<AccountDTO>
                {
                    new AccountDTO
                    {
                        Id = "1",
                        Name = "Phone"
                    },
                    new AccountDTO
                    {
                        Id = "2",
                        Name = "Travelling"
                    },
                    new AccountDTO
                    {
                        Id = "3",
                        Name = "Accounts Payable"
                    },
                }
            };
        }
    }
}
