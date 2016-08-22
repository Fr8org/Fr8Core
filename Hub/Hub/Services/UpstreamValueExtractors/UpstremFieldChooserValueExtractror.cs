using System;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Services.UpstreamValueExtractors
{
    public class UpstremFieldChooserValueExtractror : UpstreamValueExtractorBase<UpstreamFieldChooser>
    {
        protected override void ExtractUpstreamValue(UpstreamFieldChooser fieldChooser, ICrateStorage crateStorage)
        {
            fieldChooser.Value = Convert.ToString(GetValue(crateStorage, fieldChooser.SelectedItem ?? new FieldDTO(fieldChooser.selectedKey)));
            fieldChooser.selectedKey = null;
            fieldChooser.SelectedItem = null;
        }
    }
}