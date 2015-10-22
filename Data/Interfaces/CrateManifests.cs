using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public static class CrateManifests
    {
        public const int STANDARD_PAYLOAD_MANIFEST_ID = 5;
        public const string STANDARD_PAYLOAD_MANIFEST_NAME = "Standard Payload Data";

        public const int DESIGNTIME_FIELDS_MANIFEST_ID = 3;
        public const string DESIGNTIME_FIELDS_MANIFEST_NAME = "Standard Design-Time Fields";

        public const int STANDARD_CONF_CONTROLS_MANIFEST_ID = 6;
        public const string STANDARD_CONF_CONTROLS_NANIFEST_NAME = "Standard Configuration Controls";

        public const int STANDARD_EVENT_REPORT_ID = 7;
        public const string STANDARD_EVENT_REPORT_NAME = "Standard Event Report";

        public const int STANDARD_EVENT_SUBSCRIPTIONS_ID = 8;
        public const string STANDARD_EVENT_SUBSCRIPTIONS_NAME = "Standard Event Subscriptions";

        public const int STANDARD_TABLE_DATA_MANIFEST_ID = 9;
        public const string STANDARD_TABLE_DATA_MANIFEST_NAME = "Standard Table Data";

        public const int STANDARD_FILE_HANDLE_MANIFEST_ID = 10;
        public const string STANDARD_FILE_HANDLE_MANIFEST_NAME = "Standard File Handle";

        public const int STANDARD_AUTHENTICATION_ID = 12;
        public const string STANDARD_AUTHENTICATION_NAME = "Standard Authentication";

        public const int DOCUSIGN_EVENT_ID = 14;
        public const string DOCUSIGN_EVENT_NAME = "Docusign Event";

        public const int DOCUSIGN_ENVELOPE_ID = 15;
        public const string DOCUSIGN_ENVELOPE_NAME = "Docusign Envelope";
    }
}
