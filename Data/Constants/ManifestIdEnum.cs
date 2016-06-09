using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.Constants
{
    public enum MT : int
    {
        [Display(Name = "Field Description")]
        FieldDescription = 3,

        [Display(Name = "Dockyard Terminal Event or Incident Report")]
        EventOrIncidentReport = 2,

        [Display(Name = "Standard Payload Keys")]
        StandardPayloadKeys = 4,

        [Display(Name = "Standard Payload Data")]
        StandardPayloadData = 5,

        [Display(Name = "Standard UI Controls")]
        StandardConfigurationControls = 6,

        [Display(Name = "Standard Event Report")]
        StandardEventReport = 7,

        [Display(Name = "Standard Event Subscription")]
        StandardEventSubscription = 8,

        [Display(Name = "Standard Table Data")]
        StandardTableData = 9,

        [Display(Name = "Standard File Handle")]
        StandardFileHandle = 10,

        [Display(Name = "Standard Routing Directive")]
        StandardRoutingDirective = 11,

        [Display(Name = "Standard Authentication")]
        StandardAuthentication = 12,

        [Display(Name = "Standard Logging Crate")]
        StandardLoggingCrate = 13,

        [Display(Name = "Logging Data")]
        LoggingData = 10013,

        [Display(Name = "Docusign Event")]
        DocuSignEvent = 14,

        [Display(Name = "Docusign Envelope")]
        DocuSignEnvelope = 15,

        [Display(Name = "Standard Security Crate")]
        StandardSecurityCrate = 16,

        [Display(Name = "Standard Query Crate")]
        StandardQueryCrate = 17,

        [Display(Name = "Standard Email Message")]
        StandardEmailMessage = 18,

        [Display(Name = "Standard Fr8 Plans")]
        StandardFr8Plans = 19,

        [Display(Name = "Standard Fr8 Hubs")]
        StandardFr8Hubs = 20,

        [Display(Name = "Standard Fr8 Containers")]
        StandardFr8Containers = 21,

        [Display(Name = "Standard Parsing Record")]
        StandardParsingRecord = 22,

        [Display(Name = "Standard Fr8 Terminal")]
        StandardFr8Terminal = 23,

        [Display(Name = "Standard File List")]
        StandardFileList = 24,

        [Display(Name = "Standard Accounting Transaction")]
        StandardAccountTransaction = 25,

        [Display(Name = "Docusign Recipient")]
        DocuSignRecipient = 26,

        [Display(Name = "Operational State")]
        OperationalStatus = 27,

        [Display(Name = "Docusign Template")]
        DocuSignTemplate = 28,

        [Display(Name = "Chart Of Accounts")]
        ChartOfAccounts = 29,

        [Display(Name = "Manifest Description")]
        ManifestDescription = 30,

        [Display(Name = "Crate Description")]
        CrateDescription = 32,

        [Display(Name = "Typed Fields")]
        TypedFields = 33,

        [Display(Name = "External Object Handles")]
        ExternalObjectHandles = 34,

        [Display(Name = "Salesforce Event")]
        SalesforceEvent = 35,

        [Display(Name = "Docusign Envelope v2")]
        DocuSignEnvelope_v2 = 36,

        [Display(Name = "Standard Business Fact")]
        StandardBusinessFact = 37,

        [Display(Name = "Plan Template")]
        PlanTemplate = 38,
        
        [Display(Name = "Validation Results")]
        ValidationResults = 39,

        [Display(Name = "Docusign Envelope v3")]
        DocuSignEnvelope_v3 = 40,

        [Display(Name = "Advisory Messages")]
        AdvisoryMessages = 41
    }
}

