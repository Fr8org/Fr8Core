using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.BaseClasses;
using System;

namespace terminalBox.Actions
{
    public class Generate_Table_Activity_v1 : TerminalActivity<Generate_Table_Activity_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("CADEAC51-3E10-4FC8-AF5D-5265D9A8EA71"),
            Name = "Generate_Table_Activity",
            Label = "Generate Table Activity",
            Version = "1",
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.ActivityCategoryDTO
            }
        };

        private const int MaxRowNumber = 100000;

        public const string TableCrateLabel = "Table";
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBox NumberOfRows;
            public TextBlock Header;

            public ActivityUi()
            {
                Controls.Add(Header = new TextBlock());
                Controls.Add(NumberOfRows = new TextBox
                {
                    Name = nameof(NumberOfRows),
                    Label = "Number of rows",
                    Value = "3"
                });
                NumberOfRows.Events.Add(ControlEvent.RequestConfig);
            }
        }

        public Generate_Table_Activity_v1(ICrateManager crateManager) 
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            ActivityUI.Header.Value = ActivityId.ToString();
            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(TableCrateLabel);
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        protected override Task Validate()
        {
            var controlName = ActivityUI.NumberOfRows.Name;
            var rowNumber = ActivityUI.NumberOfRows.Value.Trim();
            if (string.IsNullOrWhiteSpace(rowNumber))
            {
                ValidationManager.SetError("Row number can't be empty", controlName);
            }
            int value;
            if (!int.TryParse(rowNumber, out value))
            {
                ValidationManager.SetError("Row number must be an integer number", controlName);
            }
            else if (value < 0 || value > MaxRowNumber)
            {
                ValidationManager.SetError($"Row number can't be negative or grater than {MaxRowNumber}");
            }
            return base.Validate();
        }

        public override Task Run()
        {
            var table = new StandardTableDataCM();
            for (int i = 0; i < int.Parse(ActivityUI.NumberOfRows.Value); i++)
            {
                TableRowDTO row;
                table.Table.Add(row = new TableRowDTO());
                for (int j = 0; j < 5; j++)
                {
                    row.Row.Add(new TableCellDTO
                    {
                        Cell = new KeyValueDTO($"Column {j}", $"Cell [{i}, {j}]")
                    });
                }
            }
            Payload.Add(Crate.FromContent(TableCrateLabel, table));
            return Task.FromResult(0);
        }
    }
}