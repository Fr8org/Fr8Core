using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Newtonsoft.Json;
using TerminalBase.BaseClasses;
using Utilities;

namespace terminalFr8Core.Activities
{
    public class AddPayloadManually_v1 : BaseTerminalActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "AddPayloadManually",
            Label = "Add Payload Manually",
            Category = ActivityCategory.Processors,
            Terminal = TerminalData.TerminalDTO,
            Version = "1",
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;
        private const string RunTimeCrateLabel = "ManuallyAddedPayload";
        private const string CrateSignalLabel = "Available Run Time Crates";

        public AddPayloadManually_v1() : base(false)
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
            var userDefinedPayload = JsonConvert.DeserializeObject<List<FieldDTO>>(fieldListControl.Value);
            Payload.Add(Crate.FromContent(RunTimeCrateLabel, new StandardPayloadDataCM(userDefinedPayload)));
            Success();
            return Task.FromResult(0);
        }

        public override Task Initialize()
        {
            var configurationControlsCrate = CreateControlsCrate();
            Storage.Add(configurationControlsCrate);
            //TODO do this with crateSignaller
            var availableRunTimeCrates = Crate.FromContent(CrateSignalLabel, new CrateDescriptionCM(
                new CrateDescriptionDTO
                {
                    ManifestType = MT.StandardPayloadData.GetEnumDisplayName(),
                    Label = RunTimeCrateLabel,
                    ManifestId = (int)MT.StandardPayloadData,
                    ProducedBy = "AddPayloadManually_v1"
                }), AvailabilityType.RunTime);

            Storage.Add(availableRunTimeCrates);
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
                userDefinedPayload.ForEach(x =>
                {
                    x.Value = x.Key;
                    x.Availability = AvailabilityType.RunTime;
                });
                
                Storage.RemoveByLabel(RunTimeCrateLabel);
                var crate = Crate.FromContent(RunTimeCrateLabel, new FieldDescriptionsCM() { Fields = userDefinedPayload });
                crate.Availability = AvailabilityType.RunTime;
                Storage.Add(crate);
            }

            return Task.FromResult(0);
        }

        private Crate CreateControlsCrate()
        {
            var fieldFilterPane = new FieldList
            {
                Label = "Fill the values for other actions",
                Name = "Selected_Fields",
                Required = true,
                Events = new List<ControlEvent>(){ControlEvent.RequestConfig}
            };

            return PackControlsCrate(fieldFilterPane);
        }

        
    }
}