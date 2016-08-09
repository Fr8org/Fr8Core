using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Newtonsoft.Json;

namespace terminalFr8Core.Activities
{
    public class Add_Payload_Manually_v1 : ExplicitTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("315c3603-eb27-4217-a07e-f5c5a52bbfc7"),
            Name = "Add_Payload_Manually",
            Label = "Add Payload Manually",
            Terminal = TerminalData.TerminalDTO,
            Version = "1",
            MinPaneWidth = 330,
            Categories = new[]
            {
                ActivityCategories.Process,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        private const string RunTimeCrateLabel = "ManuallyAddedPayload";

        public Add_Payload_Manually_v1(ICrateManager crateManager)
            : base(crateManager)
        {

        }

        public override Task Run()
        {
            var fieldListControl = GetControl<FieldList>("Selected_Fields");
            if (fieldListControl == null)
            {
                RaiseError("Could not find FieldListControl.");
                return Task.FromResult(0);
            }
            var userDefinedPayload = JsonConvert.DeserializeObject<List<KeyValueDTO>>(fieldListControl.Value);
            Payload.Add(Crate.FromContent(RunTimeCrateLabel, new StandardPayloadDataCM(userDefinedPayload)));
            Success();
            return Task.FromResult(0);
        }

        public override Task Initialize()
        {
            CreateControls();
            return Task.FromResult(0);
        }

        public override Task FollowUp()
        {
            var fieldListControl = GetControl<FieldList>("Selected_Fields");
            if (fieldListControl == null)
            {
                throw new ApplicationException("Could not find FieldListControl.");
            }

            if (fieldListControl.Value != null)
            {
                var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);

                CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(RunTimeCrateLabel, true).AddFields(userDefinedPayload);
            }

            return Task.FromResult(0);
        }

        private void CreateControls()
        {
            var fieldFilterPane = new FieldList
            {
                Label = "Fill the values for other actions",
                Name = "Selected_Fields",
                Required = true,
                Events = new List<ControlEvent>(){ControlEvent.RequestConfig}
            };

            AddControl(fieldFilterPane);
        }
    }
}