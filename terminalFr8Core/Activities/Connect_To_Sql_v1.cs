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
using System;
using Fr8.Infrastructure.Data.Manifests;

namespace terminalFr8Core.Activities
{
    public class Connect_To_Sql_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("bb019231-435a-49c3-96db-ab4ae9e7fb23"),
            Name = "Connect_To_Sql",
            Label = "Connect To SQL",
            Version = "1",
            Tags = Tags.Internal,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public FindObjectHelper FindObjectHelper { get; set; }

        public Connect_To_Sql_v1(ICrateManager crateManager)
            : base(crateManager)
        {
            FindObjectHelper = new FindObjectHelper();
        }

        #region Configuration.

        private void CreateControls()
        {
            var control = new TextBox()
            {
                Label = "SQL Connection String",
                Name = "ConnectionString",
                Required = true,
                Events = new List<ControlEvent>(){ControlEvent.RequestConfig}
            };

            AddControls(control);
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
            
            CreateControls();

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
                    Storage.Add("Sql Table Definitions", new KeyValueListCM(tableDefinitions));

                    var columnTypes = FindObjectHelper.RetrieveColumnTypes(connectionString);
                    Storage.Add("Sql Column Types", new KeyValueListCM(columnTypes));

                    Storage.Add("Sql Connection String", new KeyValueListCM(new KeyValueDTO (connectionString, connectionString)));
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