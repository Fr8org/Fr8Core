using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using terminalFr8Core.Infrastructure;

namespace terminalFr8Core.Activities
{
    public class ConnectToSql_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "ConnectToSql",
            Label = "Connect To SQL",
            Category = ActivityCategory.Processors,
            Version = "1",
            Tags = Tags.Internal,
            Terminal = TerminalData.TerminalDTO,
            WebService = TerminalData.WebServiceDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public FindObjectHelper FindObjectHelper { get; set; }

        public ConnectToSql_v1(ICrateManager crateManager)
            : base(crateManager)
        {
            FindObjectHelper = new FindObjectHelper();
        }

        #region Configuration.

        private Crate CreateControlsCrate()
        {
            var control = new TextBox()
            {
                Label = "SQL Connection String",
                Name = "ConnectionString",
                Required = true,
                Events = new List<ControlEvent>(){ControlEvent.RequestConfig}
            };

            return PackControlsCrate(control);
        }

        private string ExtractConnectionString()
        {
            var connectionStringControl = GetControl<TextBox>("ConnectionString");
            return connectionStringControl.Value;
        }

        #endregion Configuration.

        public override async Task Run()
        {
            Success();
            await Task.Yield();
        }

        public override async Task Initialize()
        {
            Storage.Clear();
            Storage.Add(CreateControlsCrate());
            await Task.Yield();
        }

        public override async Task FollowUp()
        {
            RemoveControl<TextBlock>("ErrorLabel");
            Storage.RemoveByLabel("Sql Table Definitions");
            var connectionString = ExtractConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    var tableDefinitions = FindObjectHelper.RetrieveColumnDefinitions(connectionString);
                    var tableDefinitionCrate =
                        CrateManager.CreateDesignTimeFieldsCrate(
                            "Sql Table Definitions",
                            tableDefinitions.ToArray()
                        );

                    var columnTypes = FindObjectHelper.RetrieveColumnTypes(connectionString);
                    var columnTypesCrate =
                        CrateManager.CreateDesignTimeFieldsCrate(
                            "Sql Column Types",
                            columnTypes.ToArray()
                        );

                    var connectionStringFieldList = new List<FieldDTO>()
                    {
                        new FieldDTO() { Key = connectionString, Value = connectionString }
                    };
                    var connectionStringCrate =
                        CrateManager.CreateDesignTimeFieldsCrate(
                            "Sql Connection String",
                            connectionStringFieldList.ToArray()
                        );

                    Storage.Add(tableDefinitionCrate);
                    Storage.Add(columnTypesCrate);
                    Storage.Add(connectionStringCrate);
                }
                catch
                {
                    AddLabelControl(
                        "ErrorLabel",
                        "Unexpected error",
                        "Error occured while trying to fetch columns from database specified."
                    );
                }
            }

            await Task.Yield();
        }
    }
}