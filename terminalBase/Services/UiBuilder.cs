using System.Collections.Generic;
using Fr8Data.Control;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace TerminalBase.Services
{
    public class UiBuilder
    {
        /// <summary>
        /// This is a generic function for creating a CrateChooser which is suitable for most use-cases
        /// </summary>
        /// <param name="label"></param>
        /// <param name="name"></param>
        /// <param name="singleManifest"></param>
        /// <param name="requestConfig"></param>
        /// <returns></returns>
        public CrateChooser CreateCrateChooser(string name,
                                               string label,
                                               bool singleManifest,
                                               bool requestConfig = false)
        {
            var control = new CrateChooser
            {
                Label = label,
                Name = name,
                SingleManifestOnly = singleManifest,
                RequestUpstream = true
            };

            if (requestConfig)
            {
                control.Events.Add(new ControlEvent("onChange", "requestConfig"));
            }

            return control;
        }
        
        /// <summary>
        /// Creates RadioButtonGroup to enter specific value or choose value from upstream crate.
        /// </summary>
        public TextSource CreateSpecificOrUpstreamValueChooser(string label,
                                                               string controlName,
                                                               string upstreamSourceLabel = "",
                                                               string filterByTag = "",
                                                               bool addRequestConfigEvent = false,
                                                               bool requestUpstream = false,
                                                               AvailabilityType availability = AvailabilityType.NotSet)
        {
            var control = new TextSource(label, upstreamSourceLabel, controlName)
            {
                Source = new FieldSourceDTO
                {
                    Label = upstreamSourceLabel,
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields,
                    FilterByTag = filterByTag,
                    RequestUpstream = requestUpstream,
                    AvailabilityType = availability
                }
            };

            if (addRequestConfigEvent)
            {
                control.Events.Add(new ControlEvent("onChange", "requestConfig"));
            }

            return control;
        }

        public UpstreamCrateChooser CreateUpstreamCrateChooser(string name, string label, bool isMultiSelection = true)
        {
            var manifestDdlb = new DropDownList { Name = name + "_mnfst_dropdown_0", Source = new FieldSourceDTO(CrateManifestTypes.StandardDesignTimeFields, "AvailableUpstreamManifests") };
            var labelDdlb = new DropDownList { Name = name + "_lbl_dropdown_0", Source = new FieldSourceDTO(CrateManifestTypes.StandardDesignTimeFields, "AvailableUpstreamLabels") };

            var ctrl = new UpstreamCrateChooser
            {
                Name = name,
                Label = label,
                SelectedCrates = new List<CrateDetails> { new CrateDetails { Label = labelDdlb, ManifestType = manifestDdlb } },
                MultiSelection = isMultiSelection
            };

            return ctrl;
        }
    }
}
