using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Interfaces;
using Hub.Services.UpstreamValueExtractors;

namespace Hub.Services
{
    public class UpstreamDataExtractionService : IUpstreamDataExtractionService
    {
        private readonly Dictionary<Type, IUpstreamValueExtractor> _valueExtractors = new Dictionary<Type, IUpstreamValueExtractor>();
        
        public UpstreamDataExtractionService()
        {
            RegisterExtractor(new TextSourceValueExtractror());
            RegisterExtractor(new UpstremFieldChooserValueExtractror());
            RegisterExtractor(new ContainerTransitionValueExtractor());
            RegisterExtractor(new FilterPaneValueExtractor());
            RegisterExtractor(new BuildMessageAppenderValueExtractor());
        }

        private void RegisterExtractor(IUpstreamValueExtractor extractor)
        {
            _valueExtractors[extractor.ConfigurationControlType] = extractor;
        }

        public void ExtactAndAssignValues(StandardConfigurationControlsCM configurationControls, ICrateStorage payloadStorage)
        {
            if (configurationControls.Controls == null)
            {
                return;
            }

            foreach (var configurationControl in configurationControls.EnumerateControlsDefinitions())
            {
                IUpstreamValueExtractor valueExtractor;

                if (_valueExtractors.TryGetValue(configurationControl.GetType(), out valueExtractor))
                {
                    valueExtractor.ExtractUpstreamValue(configurationControl, payloadStorage);
                }
            }    
        }
    }
}
