using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using terminalQuickBooks.Infrastructure;
using terminalQuickBooks.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalQuickBooks.Actions
{
    public class Convert_TableData_To_AccountingTransactions_v1 : BaseTerminalAction
    {
        private const string ButtonGroupNamePrefix = "Line";
        private ChartOfAccounts _chartOfAccounts;
        private AccountDTO DebitLineAccount;
        private ChartOfAccountsCM chartOfAccountsCrate;
        private string AllTransactionsMemo;
        public Convert_TableData_To_AccountingTransactions_v1()
        {
            _chartOfAccounts = new ChartOfAccounts();
        }
        public async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);
            return await ProcessConfigurationRequest(curActionDO, dto => ConfigurationRequestType.Initial, authTokenDO);
        }
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            return Crate.IsStorageEmpty(curActionDO) ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
        }
        protected override Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //Grasp the user data from the controls in the follow up configuration
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                UpdateControlsData(updater.CrateStorage);
            }
            return Task.FromResult(curActionDO);
        }
        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            DebitLineAccount = new AccountDTO();
            if (curActionDO.Id != Guid.Empty)
            {
                //Get StandardTableDataCM crate using desing time
                var tableDataCrate = await GetCratesByDirection<StandardTableDataCM>(curActionDO, CrateDirection.Upstream);
                //In order to convert Table Data an upstream action needs to provide a StandardTableDataCM.
                StandardConfigurationControlsCM actionControls;
                
                if (tableDataCrate.Count != 0 || tableDataCrate.Any(x => x.Label == "DocuSignTableDataMappedToQuickbooks"))
                {
                    var curTable = tableDataCrate.Single(x => x.Label == "DocuSignTableDataMappedToQuickbooks").Content;
                    var curFirstTableRow = curTable.GetHeaderRow();
                    ValidateTableHeaderRow(curFirstTableRow);
                    actionControls = ActionUI();
                }
                //If no elements have label "DocuSignTableDataMappedToQuickbooks"
                else
                {
                    var textBlock = GenerateTextBlock("Convert Table Data to Accounting Transactions",
                        "When this Action runs, it will be expecting to find a Crate of Standard Table Data. Right now, it doesn't detect any Upstream Actions that produce that kind of Crate. Please add an action upstream (to the left) of this action that does so.",
                        "alert alert-warning");
                    actionControls = new StandardConfigurationControlsCM
                    {
                        Controls = new List<ControlDefinitionDTO>
                        {
                            textBlock
                        }
                    };

                }
                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    updater.CrateStorage.Clear();
                    updater.CrateStorage.Add(PackControls(actionControls));
                    updater.CrateStorage.AddRange(await PackSources(curActionDO, authTokenDO));
                }
            }
            else
            {
                throw new ArgumentException(
                    "Configuration requires the submission of an Action that has a real ActionId");
            }
            return curActionDO;
        }
        /// <summary>
        ///It is supposed to obtain StandardTableDataCM and user input in the form of
        /// debit line account and (optionaly) memo. The debit account is all for all transactions.
        /// </summary>
        /// <returns>
        /// The StandardAccountingTransactionCM is added to the payload.
        /// </returns>
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);
            var payloadCrates = await GetPayload(curActionDO, containerId);
            //Obtain the crate of type StandardTableDataCM and label "DocuSignTableDataMappedToQuickbooks" that holds the required information in the tabular format
            var curStandardTableDataCM = Crate.UpdateStorage(payloadCrates).CrateStorage.CrateContentsOfType<StandardTableDataCM>(x => x.Label == "DocuSignTableDataMappedToQuickbooks").Single();
            //Validate the header row format
            ValidateTableHeaderRow(curStandardTableDataCM.GetHeaderRow());
            //The check on the accounts' existence in QB is performed only once in the run method
            //ChartOfAccounts is perloaded once and the accounts are compared with the the perloaded list
            if (chartOfAccountsCrate == null || chartOfAccountsCrate.Accounts.Count == 0)
                chartOfAccountsCrate = _chartOfAccounts.GetChartOfAccounts(authTokenDO);
            var curStandardAccouningTransactionCM = GenerateTransactionCrate(curStandardTableDataCM,
                chartOfAccountsCrate);

            using (var updater = Crate.UpdateStorage(payloadCrates))
            {
                var curCrateToAdd = Crate<StandardAccountingTransactionCM>.FromContent(
                    "StandardAccountingTransactionCM", curStandardAccouningTransactionCM);
                updater.CrateStorage.Add(curCrateToAdd);
            }
            return Success(payloadCrates);
        }
        private StandardConfigurationControlsCM ActionUI()
        {
            var controls = new StandardConfigurationControlsCM();
            var accountNamePicker = new List<ControlDefinitionDTO>
            {
                GenerateTextBlock("",
                    "Each table row is a separate transaction",
                    "well well-lg"),
                new RadioButtonGroup
                {
                    Name = ButtonGroupNamePrefix + "1",
                    Label = "For Distribution Line 1, use:",
                    Radios = new List<RadioButtonOption>()
                    {
                        new RadioButtonOption()
                        {
                            Value = "The table column name",
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new TextBox()
                                {
                                    Name = "Account_One_TextBoxValue",
                                    Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")}
                                }
                            }
                        },
                        new RadioButtonOption()
                        {
                            Value = "This account",
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new DropDownList()
                                {
                                    Name = "Account_One_ChartOfAccounts",
                                    Source = new FieldSourceDTO
                                    {
                                        Label = "Available ChartOfAccounts",
                                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                    },
                                    Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")}
                                }
                            }
                        }
                    }
                },
                new RadioButtonGroup()
                {
                    Label = "    First line is:",
                    Name = "Debit/Credit",
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
                },
                new RadioButtonGroup
                {
                    Name = ButtonGroupNamePrefix + "2",
                    Label = "For Distribution Line 2, use:",
                    Radios = new List<RadioButtonOption>()
                    {
                        new RadioButtonOption()
                        {
                            Value = "The table column name",
                            Selected = false,
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new TextBox()
                                {
                                    Name = "Account_Two_TextBoxValue",
                                    Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")}
                                }
                            }
                        },
                        new RadioButtonOption()
                        {
                            Value = "This account",
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new DropDownList()
                                {
                                    Name = "Account_Two_ChartOfAccounts",
                                    Source = new FieldSourceDTO
                                    {
                                        Label = "Available ChartOfAccounts",
                                        ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                    },
                                    Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")}
                                }
                            }
                        }
                    }
                },
                new TextBox
                {
                    Label = "Memo",
                    Name = "Transaction_Memo",
                    Required = false
                }
            };
            controls.Controls = accountNamePicker;
            return controls;
        }
        private async Task<IEnumerable<Crate>> PackSources(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            var sources = new List<Crate>();
            var chartOfAccounts = _chartOfAccounts.GetChartOfAccounts(authTokenDO);
            sources.Add(
                Crate.CreateDesignTimeFieldsCrate(
                    "Available ChartOfAccounts",
                //Labda function maps AccountDTO to FieldDTO: Id->Key, Name->Value
                    chartOfAccounts.Accounts.Select(x => new FieldDTO(x.Name, x.Id)).ToArray()
                )
            );
            return sources;
        }
        /// <summary>
        /// This method should update controls data
        /// This action should be called from Crate.UpdateStorage(curActionDO)
        /// 1) Decide which line is debit depending on the value of the
        /// ButtonGroup with the Name "Debit/Credit", update the debitLineAccount values
        /// 2) Extract Memo from the TextBlock with Name "Transaction_Memo"
        /// </summary>
        private void UpdateControlsData(CrateStorage storage)
        {
            var controls = GetConfigurationControls(storage);
            var curDebitCreditButtonGroup = (RadioButtonGroup)GetControl(controls, "Debit/Credit", ControlTypes.RadioButtonGroup);
            //Depending on which button is selected (Debit or Credit) pass 1 or 2 to update the debitLineAccount private object
            DebitLineAccount = ExtractAccountFromControl(controls, curDebitCreditButtonGroup.Radios[0].Selected ? 1 : 2);
            var curMemoControl = (TextBox)GetControl(controls, "Transaction_Memo", ControlTypes.TextBox);
            AllTransactionsMemo = curMemoControl.Value;
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
            if (headerRow.Row.Count <= 3)
                throw new Exception("The StandardTableDataCM contains less than 3 columns");
            if (headerRow.Row.Any(x => x.Cell.Value == "Date") || headerRow.Row.Any(x => x.Cell.Value == "Description"))
                throw new Exception("The StandardTableDataCM does not contain Date or/and Description column");
        }
        /// <summary>
        /// Maps StandardTableDataCM manifest crate to the StandardAccountingTransactionCM manifest crate
        /// </summary>
        /// <param name="tableData">StandardTableDataCM crate</param>
        /// <param name="accounts">ChartOfAccountsCM preloaded from QuickBooks</param>
        /// <returns>StandardAccountingTransactionCM crate</returns>
        public StandardAccountingTransactionCM GenerateTransactionCrate(StandardTableDataCM tableData, ChartOfAccountsCM accounts)
        {
            var curTransactionCrate = new StandardAccountingTransactionCM();
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
                    var debitLine = GenerateFinacialLine(DebitLineAccount, curDate, curDescription, curAmount, "Debit");
                    curTransaction.TransactionDate = DateTime.Parse(curDate);
                    curTransaction.Memo = AllTransactionsMemo;
                    curTransaction.FinancialLines = new List<FinancialLineDTO> { creditLine, debitLine };
                }
                curTransactionCrate.AccountingTransactions.Add(curTransaction);
            }
            return curTransactionCrate;
        }

        public FinancialLineDTO GenerateFinacialLine(AccountDTO account, string date, string description, string amount, string debitOrCredit = "Credit")
        {
            //Validate parameters
            try
            {
                var curAmount = Decimal.Parse(amount);
            }
            catch (Exception ex)
            {
                throw new Exception("Date or amount are in the incorrect format.");
            }
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