using Data.Crates;

namespace Data.Interfaces.Manifests
{
    public static class CrateManifestTypes
    {
        public static string StandardDesignTimeFields
        {
            get { return ManifestDiscovery.Default.GetManifestType<FieldDescriptionsCM>().Type; }
        }

        public static string StandardConfigurationControls
        {
            get { return ManifestDiscovery.Default.GetManifestType<StandardConfigurationControlsCM>().Type; }
        }

        public static string StandardTableData
        {
            get { return ManifestDiscovery.Default.GetManifestType<StandardTableDataCM>().Type; }
        }

        public static string StandardFileDescription
        {
            get { return ManifestDiscovery.Default.GetManifestType<StandardFileDescriptionCM>().Type; }
        }
    }
}
