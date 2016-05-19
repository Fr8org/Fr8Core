using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalTest.Actions
{
    public class GenerateTableActivity_v1 : TestActivityBase<GenerateTableActivity_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBox NumberOfRows;
            public TextBlock Header;

            public ActivityUi()
            {
                Controls.Add(Header = new TextBlock());
                Controls.Add(NumberOfRows = new TextBox { Label = "Number of rows", Value = "3"});
                NumberOfRows.Events.Add(ControlEvent.RequestConfig);
            }
        }

        protected override Task InitializeETA()
        {
            ActivityUI.Header.Value = ActivityId.ToString();
            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>("Table");

            return Task.FromResult(0);
        }

        protected override Task ConfigureETA()
        {
            return Task.FromResult(0);
        }

        protected override Task RunETA()
        {
            Log($"Table {ActivityPayload.Label} [{ActivityId}] started");

            var tableCm = new StandardTableDataCM();

            for (int i = 0; i < int.Parse(ActivityUI.NumberOfRows.Value); i ++)
            {
                TableRowDTO row;
                tableCm.Table.Add(row = new TableRowDTO());

                for (int j = 0; j < 5; j ++)
                {
                    row.Row.Add(new TableCellDTO
                    {
                        Cell = new FieldDTO("Column " + j, $"Cell [{i}, {j}]")
                    });
                }
            }

            Payload.Add(Crate.FromContent("Table", tableCm));

            return Task.FromResult(0);
        }

        protected override ActivityTemplateDTO MyTemplate => new ActivityTemplateDTO {};

        protected override Task RunChildActivities()
        {
            Log($"{ActivityPayload.Label} [{ActivityId}] ended");

            return Task.FromResult(0);
        }
    }
}