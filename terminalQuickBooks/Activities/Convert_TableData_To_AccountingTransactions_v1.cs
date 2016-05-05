using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Task = System.Threading.Tasks.Task;
using StructureMap;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Actions
{
    public class Convert_TableData_To_AccountingTransactions_v1 :
        BaseQuickbooksTerminalActivity<Convert_TableData_To_AccountingTransactions_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBlock TableRowTextBlock { get; set; }
            public RadioButtonGroup Line1 { get; set; }
            public RadioButtonGroup DebitCredit { get; set; }
            public RadioButtonGroup Line2 { get; set; }
            public TextBox Memo { get; set; }

            public ActivityUi()
            {
                TableRowTextBlock = new TextBlock
                {
                    Label = string.Empty,
                    Value = "Each table row is a separate transaction",
                    CssClass = "well well-lg",
                    Name = nameof(TableRowTextBlock)
                };
                Controls.Add(TableRowTextBlock);

                Line1 = new RadioButtonGroup
                {
                    Name = ButtonGroupNamePrefix + "1",
                    Label = "For Distribution Line 1, use:",
                    Radios = new List<RadioButtonOption>()
                    {
                        new RadioButtonOption()
                        {
                            Value = "The table column name",
                            Selected = true,
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new TextBox()
                                {
                                    Name = "Account_One_TextBoxValue"
                                }
                            }
                        },
                        new RadioButtonOption()
                        {
                            Value = "This account",
                            Selected = false,
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new DropDownList()
                                {
                                    Name = "Account_One_ChartOfAccounts",
                                    Source = new FieldSourceDTO
                                    {
                                        Label = "Available ChartOfAccounts",
                                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                    }
                                }
                            }
                        }
                    }
                };
                Controls.Add(Line1);

                DebitCredit = new RadioButtonGroup()
                {
                    Label = "    First line is:",
                    Name = nameof(DebitCredit),
                    Radios = new List<RadioButtonOption>()
                    {
                        new RadioButtonOption()
                        {
                            Name = "Debit",
                            Value = "Debit",
                            Selected = true
                        },
                        new RadioButtonOption()
                        {
                            Name = "Credit",
                            Value = "Credit",
                            Selected = false
                        }
                    }
                };
                Controls.Add(DebitCredit);

                Line2 = new RadioButtonGroup
                {
                    Name = ButtonGroupNamePrefix + "2",
                    Label = "For Distribution Line 2, use:",
                    Radios = new List<RadioButtonOption>()
                    {
                        new RadioButtonOption()
                        {
                            Value = "The table column name",
                            Selected = true,
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new TextBox()
                                {
                                    Name = "Account_Two_TextBoxValue"
                                }
                            }
                        },
                        new RadioButtonOption
                        {
                            Value = "This account",
                            Selected = false,
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new DropDownList()
                                {
                                    Name = "Account_Two_ChartOfAccounts",
                                    Source = new FieldSourceDTO
                                    {
                                        Label = "Available ChartOfAccounts",
                                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                    }
                                }
                            }
                        }
                    }
                };
                Controls.Add(Line2);

                Memo = new TextBox
                {
                    Label = "Memo",
                    Name = nameof(Memo),
                    Required = false
                };
                Controls.Add(Memo);
            }
        }

        private const string UpsteamCrateLabel = "DocuSignTableDataMappedToQuickbooks";
        //Prefix and the OR related to it are included for testing purposes as
        //HealthMonitor authomatically changes the crate label by adding the prefix.
        private const string HealthMonitorPrefix = "HealthMonitor_UpstreamCrate_";
        private const string ButtonGroupNamePrefix = "Line";
        private readonly IChartOfAccounts _chartOfAccounts;
        private ChartOfAccountsCM ChartOfAccountsCrate;
        private string MemoText;
        private AccountDTO DebitAccount;

        public Convert_TableData_To_AccountingTransactions_v1()
        {
            _chartOfAccounts = ObjectFactory.GetInstance<IChartOfAccounts>();
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            if (CurrentActivity.Id == Guid.Empty)
                Error("Configuration requires the submission of an Action that has a real ActionId");

            //Check the availability of ChartOfAccountsCM
            var chartOfAccountsCmCrates = CurrentActivityStorage.CrateContentsOfType<ChartOfAccountsCM>();
            if (chartOfAccountsCmCrates != null && chartOfAccountsCmCrates.Any())
                ChartOfAccountsCrate = chartOfAccountsCmCrates.First();

            //Get StandardTableDataCM crate using desing time
            var tableDataCrate = await GetCratesByDirection<StandardTableDataCM>(CurrentActivity, CrateDirection.Upstream);

            if (tableDataCrate.Count != 0 || tableDataCrate.Any(x => x.Label == UpsteamCrateLabel || x.Label == HealthMonitorPrefix + UpsteamCrateLabel))
            {
                var curTable = tableDataCrate.Single(x => x.Label == UpsteamCrateLabel || x.Label == HealthMonitorPrefix + UpsteamCrateLabel).Content;
                var curFirstTableRow = curTable.GetHeaderRow();
                ValidateTableHeaderRow(curFirstTableRow);
            }
            else
            {
                Error("When this Action runs, it will be expecting to find a Crate of Standard Table Data. " +
                      "Right now, it doesn't detect any Upstream Actions that produce that kind of Crate. " +
                      "Please add an action upstream (to the left) of this action that does so.");
            }

            CurrentActivityStorage.AddRange(await PackSources());
        }

        protected override Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            MemoText = ConfigurationControls.Memo.Value;
            DebitAccount = GetDebitAccount(CurrentActivityStorage);
            return Task.FromResult(0);
        }

        /// <summary>
        ///It is supposed to obtain StandardTableDataCM and user input in the form of
        /// debit line account and (optionaly) memo. The debit account is all for all transactions.
        /// </summary>
        protected override async Task RunCurrentActivity()
        {
            AccountDTO curDebitAccount = null;
            string memoText = "Unspecified";
            // Obtain the crate of type StandardTableDataCM and label "DocuSignTableDataMappedToQuickbooks" that holds 
            // the required information in the tabular format
            var curStandardTableDataCM = GetCratesByDirection<StandardTableDataCM>(CurrentActivity, CrateDirection.Upstream)
                .Result.Single(x => x.Label == UpsteamCrateLabel || x.Label == HealthMonitorPrefix + UpsteamCrateLabel)
                .Content;
            //Validate the header row format
            ValidateTableHeaderRow(curStandardTableDataCM.GetHeaderRow());
            //The check on the accounts' existence in QB is performed only once in the run method
            //ChartOfAccounts is perloaded once and the accounts are compared with the the perloaded list
            if (ChartOfAccountsCrate == null || ChartOfAccountsCrate.Accounts.Count == 0)
                using (var crateStorage = CrateManager.GetUpdatableStorage(CurrentActivity))
                {
                    var crate = crateStorage.CrateContentsOfType<ChartOfAccountsCM>();
                    ChartOfAccountsCrate = crate != null
                        ? crate.Single()
                        : _chartOfAccounts.GetChartOfAccounts(GetQuickbooksAuthToken());
                    curDebitAccount = GetDebitAccount(crateStorage);
                    memoText = GetMemoText(crateStorage);
                    //Check that the required input is provided by the user
                    //Namely: debit/credit type
                    ValidateControls(crateStorage);
                }
            StandardAccountingTransactionCM curStandardAccouningTransactionCM;
            if (curDebitAccount != null)
            {
                curStandardAccouningTransactionCM = GenerateTransactionCrate(curStandardTableDataCM,
                    ChartOfAccountsCrate, curDebitAccount, memoText);

                var curCrateToAdd = Crate<StandardAccountingTransactionCM>.FromContent(
                    "StandardAccountingTransactionCM", curStandardAccouningTransactionCM);
                CurrentPayloadStorage.Add(curCrateToAdd);
            }
            else
            {
                Error("No Debit Line Account data provided", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
        }

        private async Task<IEnumerable<Crate>> PackSources()
        {
            var sources = new List<Crate>();
            ChartOfAccountsCM chartOfAccounts;
            //Check if private variable is null
            if (ChartOfAccountsCrate == null)
                chartOfAccounts = _chartOfAccounts.GetChartOfAccounts(GetQuickbooksAuthToken());
            else
                chartOfAccounts = ChartOfAccountsCrate;
            sources.Add(
                CrateManager.CreateDesignTimeFieldsCrate(
                    "Available ChartOfAccounts",
                    //Labda function maps AccountDTO to FieldDTO: Id->Key, Name->Value
                    chartOfAccounts.Accounts.Select(x => new FieldDTO(x.Name, x.Id)).ToArray()
                )
            );
            return sources;
        }

        private void ValidateControls(ICrateStorage storage)
        {
            var controls = GetConfigurationControls(storage);
            var curDebitCreditButtonGroup = (RadioButtonGroup)GetControl(controls, "Debit/Credit", ControlTypes.RadioButtonGroup);
            //Check that the Debit/Credit type is selected
            if (!curDebitCreditButtonGroup.Radios[0].Selected && !curDebitCreditButtonGroup.Radios[1].Selected)
                throw new Exception("No debit/credit type of the first account is provided");
        }

        /// <summary>
        /// This method should update controls data
        /// This action should be called from Crate.GetUpdatableStorage(curActivityDO)
        /// 1) Decide which line is debit depending on the value of the
        /// ButtonGroup with the Name "Debit/Credit", update the debitLineAccount values
        /// 2) Extract Memo from the TextBlock with Name "Transaction_Memo"
        /// </summary>
        private string GetMemoText(ICrateStorage storage)
        {
            var controls = GetConfigurationControls(storage);
            var curMemoControl = (TextBox)GetControl(controls, "Transaction_Memo", ControlTypes.TextBox);
            return curMemoControl.Value;
        }

        /// <summary>
        /// This method has two purposes:
        /// 1)It extracts the Name or Name & Id from the control crate;
        /// 2)If the Id had not been extracted, it finds it from ChartOfAccountsCM crate
        /// </summary>
        /// <remarks>It is assumed that the method is called from using.
        /// It is also assumed that ChartOfAccountCM is in the storage</remarks>
        private AccountDTO GetDebitAccount(ICrateStorage storage)
        {
            var debitAccount = new AccountDTO();
            var controls = GetConfigurationControls(storage);
            var accountsCrate = storage.CrateContentsOfType<ChartOfAccountsCM>().Single();
            var curDebitCreditButtonGroup = (RadioButtonGroup)GetControl(controls, "Debit/Credit", ControlTypes.RadioButtonGroup);
            //Depending on which button is selected (Debit or Credit) pass 1 or 2 to update the debitLineAccount private object
            debitAccount = ExtractAccountFromControl(controls, curDebitCreditButtonGroup.Radios[0].Selected ? 1 : 2);
            if (debitAccount.Id == null)
                debitAccount.Id = accountsCrate.Accounts.First(a => a.Name == debitAccount.Name).Id;
            return debitAccount;
        }

        /// <summary>
        /// This method extracts account data
        /// assuming that the account exists in QuickBooks
        /// as the mapping should have been done by previous actions
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        private AccountDTO ExtractAccountFromControl(StandardConfigurationControlsCM controls, int lineNumber)
        {
            //Prepare a QuickBooks account to return
            var curQBAccount = new AccountDTO();
            //Obtain a ButtonGroup control with defined name
            var curButtonGroup = (RadioButtonGroup)GetControl(controls, ButtonGroupNamePrefix + lineNumber, ControlTypes.RadioButtonGroup);
            //If text block is selected as input type
            if (curButtonGroup.Radios[0].Selected)
            {
                //This account should be varified on run-time as no Id is provided in design-time
                var curTextBox = (TextBox)curButtonGroup.Radios[0].Controls[0];
                curQBAccount.Name = curTextBox.Value;
                return curQBAccount;
            }
            //If DropDownList is prefered
            var curDropDownList = (DropDownList)curButtonGroup.Radios[1].Controls[1];
            var curSelectedValue = curDropDownList.ListItems.First(x => x.Selected);
            curQBAccount.Name = curSelectedValue.Value;
            curQBAccount.Id = curSelectedValue.Key;
            return curQBAccount;
        }

        /// <summary>
        /// The columns are assumed to have the following titles:
        /// Date, Description, AnyExpenseType (like Food, Travel, Hotel, etc), AnyExpenseType, etc.
        /// </summary>
        /// <param name="headerRow"></param>
        private void ValidateTableHeaderRow(TableRowDTO headerRow)
        {
            if (headerRow.Row.Count < 3)
                throw new Exception("The StandardTableDataCM contains less than 3 columns");
            if (headerRow.Row.All(x => x.Cell.Value != "Date") || headerRow.Row.All(x => x.Cell.Value != "Description"))
                throw new Exception("The StandardTableDataCM does not contain Date or/and Description column");
        }

        /// <summary>
        /// Maps StandardTableDataCM manifest crate to the StandardAccountingTransactionCM manifest crate
        /// </summary>
        /// <param name="tableData">StandardTableDataCM crate</param>
        /// <param name="accounts">ChartOfAccountsCM preloaded from QuickBooks</param>
        /// <returns>StandardAccountingTransactionCM crate</returns>
        public StandardAccountingTransactionCM GenerateTransactionCrate(StandardTableDataCM tableData, ChartOfAccountsCM accounts, AccountDTO debitAccount, string memoText = "Unspecified")
        {
            var curTransactionCrate = new StandardAccountingTransactionCM();
            curTransactionCrate.AccountingTransactions = new List<StandardAccountingTransactionDTO>();
            //Iterate through each column begining from the #3
            var curNumOfColumns = tableData.GetHeaderRow().Row.Count;
            var curHeaderRow = tableData.GetHeaderRow().Row;
            //As there is no Column object C++-style loop is used
            for (int i = 2; i < curNumOfColumns; i++)
            {
                var curAccountName = curHeaderRow[i].Cell.Value;
                var curAccount = accounts.Accounts.First(x => x.Name == curAccountName);
                var curTransaction = new StandardAccountingTransactionDTO();
                //Iterate throught each table row begining from the second
                foreach (var curRow in tableData.Table.Skip(1))
                {
                    var curCellList = curRow.Row;
                    var curDate = curCellList[0].Cell.Value;
                    var curDescription = curCellList[1].Cell.Value;
                    var curAmount = curCellList[i].Cell.Value;
                    var creditLine = GenerateFinacialLine(curAccount, curDate, curDescription, curAmount, "Credit");
                    var debitLine = GenerateFinacialLine(debitAccount, curDate, curDescription, curAmount, "Debit");
                    curTransaction.TransactionDate = DateTime.Parse(curDate, new CultureInfo("en-US"));
                    curTransaction.Memo = memoText;
                    curTransaction.FinancialLines = new List<FinancialLineDTO> { creditLine, debitLine };
                }
                curTransactionCrate.AccountingTransactions.Add(curTransaction);
            }
            return curTransactionCrate;
        }

        public FinancialLineDTO GenerateFinacialLine(AccountDTO account, string date, string description, string amount, string debitOrCredit = "Credit")
        {
            return new FinancialLineDTO
            {
                AccountId = account.Id,
                AccountName = account.Name,
                Amount = amount,
                Description = description,
                DebitOrCredit = debitOrCredit
            };
        }
    }
}