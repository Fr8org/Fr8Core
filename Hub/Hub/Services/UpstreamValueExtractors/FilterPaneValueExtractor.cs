using System.Collections.Generic;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;

namespace Hub.Services.UpstreamValueExtractors
{
    public class FilterPaneValueExtractor : UpstreamValueExtractorBase<FilterPane>
    {
        protected override void ExtractUpstreamValue(FilterPane filterPaneControl, ICrateStorage crateStorage)
        {
            var filterDataDTO = JsonConvert.DeserializeObject<FilterDataDTO>(filterPaneControl.Value);

            filterPaneControl.ResolvedUpstreamFields = new List<KeyValueDTO>();

            foreach (var condition in filterDataDTO.Conditions)
            {
                var fieldValue = GetValue(crateStorage, new FieldDTO(condition.Field))?.ToString();

                if (!string.IsNullOrEmpty(fieldValue))
                {
                    filterPaneControl.ResolvedUpstreamFields.Add(new KeyValueDTO(condition.Field, fieldValue));
                }
            }
        }
    }
}
