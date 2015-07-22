using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Core.Interfaces;

namespace Core.Utilities
{
    public class DocusignXml : IDocusignXml
    {
        /// <summary>
        /// Extracts envelopeId from DocuSign XML message.
        /// </summary>
        /// <returns>envelopeId.</returns>
        public string GetEnvelopeIdFromXml(string xmlPayload)
        {
            if (string.IsNullOrEmpty(xmlPayload))
                throw new ArgumentNullException("xmlPayload");

            string xPath = @"/a:DocuSignEnvelopeInformation/a:EnvelopeStatus/a:EnvelopeID[1]/text()";
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlPayload);

            //Handle default namespace in XML with XmlNamespaceManager. 
            //Without this trick xPath will not find the needed element. 
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("a", "http://www.docusign.net/API/3.0");
            var node = xmlDoc.SelectSingleNode(xPath, nsManager);

            if (node != null)
                return node.Value;
            else
                throw new ArgumentException("EnvelopeId is not found in XML payload.");
        }

    }
}
