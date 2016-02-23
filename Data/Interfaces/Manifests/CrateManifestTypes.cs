using Data.Crates;

namespace Data.Interfaces.Manifests
{
    public static class CrateManifestTypes
    {
        public static string StandardDesignTimeFields
        {
            get { return ManifestDiscovery.Default.GetManifestType<StandardDesignTimeFieldsCM>().Type; }
        }

        public static string StandardConfigurationControls
        {
            get { return ManifestDiscovery.Default.GetManifestType<StandardConfigurationControlsCM>().Type; }
        }

        public static string StandardQueryFields
        {
            get { return ManifestDiscovery.Default.GetManifestType<StandardQueryFieldsCM>().Type; }
        }
    }
}
