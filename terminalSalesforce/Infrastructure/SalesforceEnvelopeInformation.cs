using System;
using System.Xml.Serialization;

namespace terminalSalesforce.Infrastructure
{
    [XmlRoot(ElementName = "sObject", Namespace = "http://soap.sforce.com/2005/09/outbound")]
    [XmlInclude(typeof(Account))]
    [XmlInclude(typeof(Case))]
    [XmlInclude(typeof(Contact))]
    [XmlInclude(typeof(Contract))]
    [XmlInclude(typeof(Document))]
    [XmlInclude(typeof(Lead))]
    [XmlInclude(typeof(Opportunity))]
    [XmlInclude(typeof(Product2))]
    public class SObject
    {
        [XmlElement(ElementName = "Id", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
        public string Id { get; set; }
        [XmlElement(ElementName = "CreatedById", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
        public string CreatedById { get; set; }
        [XmlElement(ElementName = "LastName", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
        public string LastName { get; set; }
        [XmlElement(ElementName = "OwnerId", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
        public string OwnerId { get; set; }
        [XmlElement(ElementName = "CreatedDate", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
        public DateTime CreatedDate { get; set; }
        [XmlElement(ElementName = "LastModifiedDate", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
        public DateTime LastModifiedDate { get; set; }
        [XmlAttribute(AttributeName = "type", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "sf", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Sf { get; set; }
    }

    [XmlRoot(Namespace = "http://soap.sforce.com/2005/09/outbound")]
    [XmlType(TypeName = "Account", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
    public class Account : SObject
    {
    }

    [XmlRoot(Namespace = "http://soap.sforce.com/2005/09/outbound")]
    [XmlType(TypeName = "Case", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
    public class Case : SObject
    {
    }

    [XmlRoot(Namespace = "http://soap.sforce.com/2005/09/outbound")]
    [XmlType(TypeName = "Contact", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
    public class Contact : SObject
    {
    }

    [XmlRoot(Namespace = "http://soap.sforce.com/2005/09/outbound")]
    [XmlType(TypeName = "Contract", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
    public class Contract : SObject
    {
    }

    [XmlRoot(Namespace = "http://soap.sforce.com/2005/09/outbound")]
    [XmlType(TypeName = "Document", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
    public class Document : SObject
    {
    }

    [XmlRoot(Namespace = "http://soap.sforce.com/2005/09/outbound")]
    [XmlType(TypeName = "Lead", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
    public class Lead : SObject
    {
    }

    [XmlRoot(Namespace = "http://soap.sforce.com/2005/09/outbound")]
    [XmlType(TypeName = "Opportunity", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
    public class Opportunity : SObject
    {
    }

    [XmlRoot(Namespace = "http://soap.sforce.com/2005/09/outbound")]
    [XmlType(TypeName = "Product2", Namespace = "urn:sobject.enterprise.soap.sforce.com")]
    public class Product2 : SObject
    {
    }

    [XmlRoot(ElementName = "Notification", Namespace = "http://soap.sforce.com/2005/09/outbound")]
    public class Notification
    {
        [XmlElement(ElementName = "Id", Namespace = "http://soap.sforce.com/2005/09/outbound")]
        public string Id { get; set; }
        [XmlElement(ElementName = "sObject", Namespace = "http://soap.sforce.com/2005/09/outbound")]
        public SObject SObject { get; set; }
    }

    [XmlRoot(ElementName = "notifications", Namespace = "http://soap.sforce.com/2005/09/outbound")]
    public class Notifications
    {
        [XmlElement(ElementName = "OrganizationId", Namespace = "http://soap.sforce.com/2005/09/outbound")]
        public string OrganizationId { get; set; }
        [XmlElement(ElementName = "ActionId", Namespace = "http://soap.sforce.com/2005/09/outbound")]
        public string ActivityId { get; set; }
        [XmlElement(ElementName = "SessionId", Namespace = "http://soap.sforce.com/2005/09/outbound")]
        public string SessionId { get; set; }
        [XmlElement(ElementName = "EnterpriseUrl", Namespace = "http://soap.sforce.com/2005/09/outbound")]
        public string EnterpriseUrl { get; set; }
        [XmlElement(ElementName = "PartnerUrl", Namespace = "http://soap.sforce.com/2005/09/outbound")]
        public string PartnerUrl { get; set; }
        [XmlElement(ElementName = "Notification", Namespace = "http://soap.sforce.com/2005/09/outbound")]
        public Notification[] NotificationList { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Body
    {
        [XmlElement(ElementName = "notifications", Namespace = "http://soap.sforce.com/2005/09/outbound")]
        public Notifications Notifications { get; set; }
    }

    [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope
    {
        [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public Body Body { get; set; }
        [XmlAttribute(AttributeName = "soapenv", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Soapenv { get; set; }
        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
    }
}
