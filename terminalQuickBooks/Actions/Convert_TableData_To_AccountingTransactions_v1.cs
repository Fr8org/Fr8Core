using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Intuit.Ipp.Data;
using terminalQuickBooks.Infrastructure;
using terminalQuickBooks.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;
using Task = System.Threading.Tasks.Task;

namespace terminalQuickBooks.Actions
{
    public class Convert_TableData_To_AccountingTransactions_v1 : BaseTerminalAction
    {
        private const string UpsteamCrateLabel = "DocuSignTableDataMappedToQuickbooks";
        //Prefix and the OR related to it are included for testing purposes as
        //HealthMonitor authomatically changes the crate label by adding the prefix.
        private const string HealthMonitorPrefix = "HealthMonitor_UpstreamCrate_";
        private const string ButtonGroupNamePrefix = "Line";
        private ChartOfAccounts _chartOfAccounts;
        private ChartOfAccountsCM ChartOfAccountsCrate;
        private string MemoText;
        private AccountDTO DebitAccount;
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
                MemoText = GetMemoText(updater.CrateStorage);
                DebitAccount = GetDebitAccount(updater.CrateStorage);
            }
            return Task.FromResult(curActionDO);
        }
        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActionDO.Id != Guid.Empty)
            {
                //Check the availability of ChartOfAccountsCM
                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    var crate = updater.CrateStorage.CrateContentsOfType<ChartOfAccountsCM>();
                    if (crate != null && crate.Any())
                        ChartOfAccountsCrate = crate.First();
                }
                //Get StandardTableDataCM crate using desing time
                var tableDataCrate = await GetCratesByDirection<StandardTableDataCM>(curActionDO, CrateDirection.Upstream);
                //In order to convert Table Data an upstream action needs to provide a StandardTableDataCM.
                StandardConfigurationControlsCM actionControls;
                if (tableDataCrate.Count != 0 || tableDataCrate.Any(x => x.Label == UpsteamCrateLabel || x.Label == HealthMonitorPrefix + UpsteamCrateLabel))
                {
                    var curTable = tableDataCrate.Single(x => x.Label == UpsteamCrateLabel || x.Label == HealthMonitorPrefix + UpsteamCrateLabel).Content;
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
                    updater.CrateStorage.Add(PackControls(actionControls));
                    updater.CrateStorage.AddRange(await PackSources(authTokenDO));
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
            
            //CheckAuthentication(authTokenDO);
            AccountDTO curDebitAccount = null;
            string memoText="Unspecified";
            var payloadCrates = await GetPayload(curActionDO, containerId);
            //Obtain the crate of type StandardTableDataCM and label "DocuSignTableDataMappedToQuickbooks" that holds the required information in the tabular format
            var curStandardTableDataCM = GetCratesByDirection<StandardTableDataCM>(curActionDO, CrateDirection.Upstream).Result.Single(x => x.Label == UpsteamCrateLabel || x.Label == HealthMonitorPrefix + UpsteamCrateLabel).Content;
            //Validate the header row format
            ValidateTableHeaderRow(curStandardTableDataCM.GetHeaderRow());
            //The check on the accounts' existence in QB is performed only once in the run method
            //ChartOfAccounts is perloaded once and the accounts are compared with the the perloaded list
            if (ChartOfAccountsCrate == null || ChartOfAccountsCrate.Accounts.Count == 0)
                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    var crate = updater.CrateStorage.CrateContentsOfType<ChartOfAccountsCM>();
                    ChartOfAccountsCrate = crate != null
                        ? crate.Single()
                        : _chartOfAccounts.GetChartOfAccounts(authTokenDO);
                    curDebitAccount = GetDebitAccount(updater.CrateStorage);
                    memoText = GetMemoText(updater.CrateStorage);
                    //Check that the required input is provided by the user
                    //Namely: debit/credit type
                    ValidateControls(updater.CrateStorage);
                }
            StandardAccountingTransactionCM curStandardAccouningTransactionCM;
            if (curDebitAccount != null)
            {
                curStandardAccouningTransactionCM = GenerateTransactionCrate(curStandardTableDataCM,
                    ChartOfAccountsCrate, curDebitAccount, memoText);
                using (var updater = Crate.UpdateStorage(payloadCrates))
                {
                    var curCrateToAdd = Crate<StandardAccountingTransactionCM>.FromContent(
                        "StandardAccountingTransactionCM", curStandardAccouningTransactionCM);
                    updater.CrateStorage.Add(curCrateToAdd);
                }
                return Success(payloadCrates);
            }
            return Error(payloadCrates, "No Debit Line Account data provided", ActionErrorCode.DESIGN_TIME_DATA_MISSING);
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
                            Selected = true,
                            Controls = new List<ControlDefinitionDTO>
                            {
                                new TextBox()
                                {
                                    Name = "Account_Two_TextBoxValue"
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
        private async Task<IEnumerable<Crate>> PackSources(AuthorizationTokenDO authTokenDO)
        {
            var sources = new List<Crate>();
            ChartOfAccountsCM chartOfAccounts;
            //Check if private variable is null
            if (ChartOfAccountsCrate == null)
                chartOfAccounts = _chartOfAccounts.GetChartOfAccounts(authTokenDO);
            else
                chartOfAccounts = ChartOfAccountsCrate;
            sources.Add(
                Crate.CreateDesignTimeFieldsCrate(
                    "Available ChartOfAccounts",
                //Labda function maps AccountDTO to FieldDTO: Id->Key, Name->Value
                    chartOfAccounts.Accounts.Select(x => new FieldDTO(x.Name, x.Id)).ToArray()
                )
            );
            return sources;
        }
        private void ValidateControls(CrateStorage storage)
        {
            var controls = GetConfigurationControls(storage);
            var curDebitCreditButtonGroup = (RadioButtonGroup)GetControl(controls, "Debit/Credit", ControlTypes.RadioButtonGroup);
            //Check that the Debit/Credit type is selected
            if (!curDebitCreditButtonGroup.Radios[0].Selected && !curDebitCreditButtonGroup.Radios[1].Selected)
                throw new Exception("No debit/credit type of the first account is provided");
        }
        /// <summary>
        /// This method should update controls data
        /// This action should be called from Crate.UpdateStorage(curActionDO)
        /// 1) Decide which line is debit depending on the value of the
        /// ButtonGroup with the Name "Debit/Credit", update the debitLineAccount values
        /// 2) Extract Memo from the TextBlock with Name "Transaction_Memo"
        /// </summary>
        private string GetMemoText(CrateStorage storage)
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
        private AccountDTO GetDebitAccount(CrateStorage storage)
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
                    curTransaction.TransactionDate = DateTime.Parse(curDate);
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