using System.Xml.Serialization;

namespace terminalDocuSign.Infrastructure.DocuSignParserModels
{
    [XmlRoot(ElementName = "RecipientStatuses")]
    public class RecipientStatuses
    {
        [XmlElement("RecipientStatus")]
        public RecipientStatus[] Statuses { get; set; }
    }
}