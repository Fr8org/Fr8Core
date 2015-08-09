using System.Xml.Serialization;

namespace Data.Entities.DocuSignParserModels
{
    [XmlRoot(ElementName = "RecipientStatuses")]
    public class RecipientStatus
    {
        [XmlElement("RecipientId")]
        public string Id { get; set; }

        [XmlElement("Status")]
        public string Status { get; set; }
    }
}