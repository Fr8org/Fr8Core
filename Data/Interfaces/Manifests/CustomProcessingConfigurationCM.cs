using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    public class CustomProcessingConfigurationCM : Manifest
    {
        public CustomProcessingConfigurationCM()
            : base(Constants.MT.CustomProcessingConfiguration)
        {
            AllowChildren = false;
        }

        public CustomProcessingConfigurationCM(bool allowChildren) : this()
        {
            AllowChildren = allowChildren;
        }

        public bool AllowChildren { get; set; }
    }
}
