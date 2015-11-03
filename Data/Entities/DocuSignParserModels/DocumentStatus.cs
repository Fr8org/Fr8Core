using System.Xml.Serialization;

namespace Data.Entities.DocuSignParserModels
{
    [XmlRoot(ElementName = "DocumentStatus")]
    public class DocumentStatus
    {
        [XmlElement("ID")]
        public string Id { get; set; }
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("TemplateName")]
        public string TemplateName { get; set; }
        [XmlElement("Sequence")]
        public string Sequence { get; set; }
    }
}
