using System.Xml.Serialization;

namespace terminalDocuSign.Infrastructure.DocuSignParserModels
{
    [XmlRoot(ElementName = "DocumentStatuses")]
    public class DocumentStatuses
    {
        [XmlElement("DocumentStatus")]
        public DocumentStatus[] Statuses { get; set; }
    }
}
