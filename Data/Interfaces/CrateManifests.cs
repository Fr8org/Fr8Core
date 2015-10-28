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

        public const int STANDARD_ROUTING_DIRECTIVE_MANIFEST_ID = 11;
        public const string STANDARD_ROUTING_DIRECTIVE_MANIFEST_NAME = "Standard Routing Directive";
       
        public const int STANDARD_LOGGING_MANIFEST_ID = 13;

        public const string STANDARD_LOGGING_MANIFEST_NAME = "Standard Logging Crate";
        public const int STANDARD_SECURITY_EVENTS_MANIFEST_ID = 16;
        public const string STANDARD_SECURITY_EVENTS_MANIFEST_NAME = "Standard Security Crate";
        public const int STANDARD_QUERY_MANIFEST_ID = 17;
        public const string STANDARD_QUERY_MANIFEST_NAME = "Standard Query Crate";
        public const int STANDARD_EMAIL_MESSAGE_MANIFEST_ID = 18;
        public const string STANDARD_EMAIL_MESSAGE_MANIFEST_NAME = "Standard Email Message";
        public const int STANDARD_FR8_ROUTES_MANIFEST_ID = 19;
        public const string STANDARD_FR8_ROUTES_MANIFEST_NAME = "Standard Fr8 Routes";
        public const int STANDARD_FR8_HUBS_MANIFEST_ID = 20;
        public const string STANDARD_FR8_HUBS_MANIFEST_NAME = "Standard Fr8 Hubs";
        public const int STANDARD_FR8_CONTAINERS_MANIFEST_ID = 21;
        public const string STANDARD_FR8_CONTAINERS_MANIFEST_NAME = "Standard Fr8 Containers";
        public const int STANDARD_PARSING_RECORD_MANIFEST_ID = 22;
        public const string STANDARD_PARSING_RECORD__MANIFEST_NAME = "Standard Parsing Record";
        public const int STANDARD_FR8_TERMINAL_MANIFEST_ID = 23;
        public const string STANDARD_FR8_TERMINAL_MANIFEST_NAME = "Standard Fr8 Terminal";
        public const int STANDARD_FILE_LIST_MANIFEST_ID = 24;
        public const string STANDARD_FILE_LIST_MANIFEST_NAME = "Standard File List";


        public static Dictionary<int, string> MANIFEST_CLASS_MAPPING_DICTIONARY = new Dictionary<int, string>
        {  
            {
                3, 
                "StandardDesignTimeFieldsCM"
            },
            {
                5, 
                "StandardPayloadDataCM"
            },
             {
                6, 
                "StandardRoutingDirectiveCM"
            }
            ,
             {
                7, 
                "EventReportCM"
            }
            ,
             {
                8, 
                "EventSubscriptionCM"
            },
             {
                9, 
                "StandardTableDataCM"
            },
            {
                10, 
                "StandardFileHandleMS"
            },
             {
                11, 
                "StandardRoutingDirectiveCM"
            },
            {
                12, 
                "StandardAuthenticationCM"
            },
            {
                13, 
                "StandardLoggingCM"
            },
            {
                14, 
                "DocuSignEnvelopeCM"
            },
            {
                15, 
                "DocuSignEventCM"
            },
            {
                16, 
                "StandardSecurityCM"
            },
            {
                17, 
                "StandardQueryCM"
            },
            {
                18, 
                "StandardEmailMessageCM"
            },
            {
                19, 
                "StandardFr8RoutesCM"
            },
            {
                20, 
                "StandardFr8HubsCM"
            },
            {
                21, 
                "StandardFr8ContainersCM"
            },
            {
                22, 
                "StandardParsingRecordCM"
            },
            {
                23, 
                "Fr8TerminalCM"
            },
            {
                24, 
                "StandardFileListCM"
            },
        };

    }
}
