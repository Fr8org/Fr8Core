using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using TerminalBase.BaseClasses;

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

        protected override Task Initialize(CrateSignaller crateSignaller)
        {
            ConfigurationControls.Header.Value = CurrentActivity.Id.ToString();
            crateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>("Table");

            return Task.FromResult(0);
        }

        protected override Task Configure(CrateSignaller crateSignaller)
        {
            return Task.FromResult(0);
        }

        protected override Task RunCurrentActivity()
        {
            Log($"Table {CurrentActivity.Label} [{CurrentActivity.Id}] started");

            var tableCm = new StandardTableDataCM();

            for (int i = 0; i < int.Parse(ConfigurationControls.NumberOfRows.Value); i ++)
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

            CurrentPayloadStorage.Add(Crate.FromContent("Table", tableCm));

            return Task.FromResult(0);
        }

        protected override Task RunChildActivities()
        {
            Log($"{CurrentActivity.Label} [{CurrentActivity.Id}] ended");

            return Task.FromResult(0);
        }
    }
}