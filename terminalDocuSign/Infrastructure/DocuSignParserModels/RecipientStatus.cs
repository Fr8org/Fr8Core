using System.Xml.Serialization;

namespace terminalDocuSign.Infrastructure.DocuSignParserModels
{
    [XmlRoot(ElementName = "RecipientStatuses")]
    public class RecipientStatus
    {
        [XmlElement("RecipientId")]
        public string Id { get; set; }

        [XmlElement("Status")]
        public string Status { get; set; }

        [XmlElement("UserName")]
        public string UserName { get; set; }

        [XmlElement("Email")]
        public string Email { get; set; }

        [XmlElement("Sent")]
        public string SentDate { get; set; }

        [XmlElement("Delivered")]
        public string DeliveredDate { get; set; }
    }
}