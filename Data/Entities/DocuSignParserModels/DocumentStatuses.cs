using System.Xml.Serialization;

namespace Data.Entities.DocuSignParserModels
{
    [XmlRoot(ElementName = "DocumentStatuses")]
    public class DocumentStatuses
    {
        [XmlElement("DocumentStatus")]
        public DocumentStatus[] Statuses { get; set; }
    }
}
