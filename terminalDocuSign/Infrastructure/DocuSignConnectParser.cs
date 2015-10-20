using System;
using System.IO;
using System.Xml.Serialization;
using Data.Entities.DocuSignParserModels;

namespace terminalDocuSign.Infrastructure
{
    public static class DocuSignConnectParser
    {
        /// <summary>
        /// Extracts envelopeId from DocuSign XML message.
        /// </summary>
        /// <returns>envelopeId.</returns>
        public static DocuSignEnvelopeInformation GetEnvelopeInformation(string xmlPayload)
        {
            if (string.IsNullOrEmpty(xmlPayload))
                throw new ArgumentNullException("xmlPayload");

            DocuSignEnvelopeInformation docusignEnvelopeInfo;
            var serializer = new XmlSerializer(typeof (DocuSignEnvelopeInformation));
            using (var reader = new StringReader(xmlPayload))
            {
                docusignEnvelopeInfo = (DocuSignEnvelopeInformation) serializer.Deserialize(reader);
            }

            if (string.IsNullOrEmpty(docusignEnvelopeInfo.EnvelopeStatus.EnvelopeId))
                throw new ArgumentException("EnvelopeId is not found in XML payload.");

            return docusignEnvelopeInfo;
        }
    }
}