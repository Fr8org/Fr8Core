using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using TerminalBase.BaseClasses;

namespace terminalBox.Actions
{
    public class GenerateTableActivity_v1 : EnhancedTerminalActivity<GenerateTableActivity_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public TextBox NumberOfRows;
            public TextBlock Header;

            public ActivityUi()
            {
                Controls.Add(Header = new TextBlock());
                Controls.Add(NumberOfRows = new TextBox { Label = "Number of rows", Value = "3" });
                NumberOfRows.Events.Add(ControlEvent.RequestConfig);
            }
        }

        public GenerateTableActivity_v1()
            : base(false)
        {
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
            var tableCm = new StandardTableDataCM();

            for (int i = 0; i < int.Parse(ConfigurationControls.NumberOfRows.Value); i++)
            {
                TableRowDTO row;
                tableCm.Table.Add(row = new TableRowDTO());

                for (int j = 0; j < 5; j++)
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
    }
}