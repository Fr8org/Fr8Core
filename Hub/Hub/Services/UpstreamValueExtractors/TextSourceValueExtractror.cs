using System;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Services.UpstreamValueExtractors
{
    public class TextSourceValueExtractror : UpstreamValueExtractorBase<TextSource>
    {
        protected override void ExtractUpstreamValue(TextSource textSource, ICrateStorage crateStorage)
        {
            if (textSource.ValueSource != TextSource.UpstreamValueSrouce)
            {
                return;
            }

            if (textSource.HasUpstreamValue)
            {
                textSource.TextValue = Convert.ToString(GetValue(crateStorage, textSource.SelectedItem ?? new FieldDTO(textSource.selectedKey)));
                textSource.ValueSource = TextSource.SpecificValueSource;
                textSource.selectedKey = null;
                textSource.SelectedItem = null;
            }
        }
    }
}