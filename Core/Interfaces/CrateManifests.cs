using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public static class CrateManifests
    {
        public const int STANDARD_PAYLOAD_MANIFEST_ID = 5;
        public const string STANDARD_PAYLOAD_MANIFEST_NAME = "Standard Payload Data";

        public const int DESIGNTIME_FIELDS_MANIFEST_ID = 3;
        public const string DESIGNTIME_FIELDS_MANIFEST_NAME = "Standard Design-Time Fields";

        public const int STANDARD_CONF_CONTROLS_MANIFEST_ID = 6;
        public const string STANDARD_CONF_CONTROLS_NANIFEST_NAME = "Standard Configuration Controls";

        public const int STANDARD_TABLE_DATA_MANIFEST_ID = 9;
        public const string STANDARD_TABLE_DATA_MANIFEST_NAME = "Standard Table Data";

        public const int STANDARD_FILE_HANDLE_MANIFEST_ID = 10;
        public const string STANDARD_FILE_HANDLE_MANIFEST_NAME = "Standard File Handle";
    }
}
