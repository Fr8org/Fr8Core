using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;

namespace terminalBox.Actions
{
    public class Generate_Table_Activity_v1 : TerminalActivity<Generate_Table_Activity_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Generate_Table_Activity",
            Label = "Generate Table Activity",
            Category = ActivityCategory.Receivers,
            Version = "1",
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[] { ActivityCategories.Receive }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

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

        public Generate_Table_Activity_v1(ICrateManager crateManager) 
            : base(crateManager)
        {
        }

        public override Task Initialize()
        {
            ActivityUI.Header.Value = ActivityId.ToString();
            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>("Table");
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            return Task.FromResult(0);
        }

        public override Task Run()
        {
            var tableCm = new StandardTableDataCM();

            for (int i = 0; i < int.Parse(ActivityUI.NumberOfRows.Value); i++)
            {
                TableRowDTO row;
                tableCm.Table.Add(row = new TableRowDTO());

                for (int j = 0; j < 5; j++)
                {
                    row.Row.Add(new TableCellDTO
                    {
                        Cell = new KeyValueDTO("Column " + j, $"Cell [{i}, {j}]")
                    });
                }
            }

            Payload.Add(Crate.FromContent("Table", tableCm));

            return Task.FromResult(0);
        }
    }
}