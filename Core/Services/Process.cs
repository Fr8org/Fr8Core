using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;
using Utilities.Logging;

namespace Core.Services
{
    public class Process : IProcess
    {
        AlertReporter _alertReporter;

        public Process(AlertReporter alertReporter)
        {
            _alertReporter = alertReporter;
        }

        /// <summary>
        /// The method processes incoming notifications from DocuSign. 
        /// </summary>
        /// <param name="userId">UserId received from DocuSign.</param>
        /// <param name="xmlPayload">XML content received from DocuSign.</param>
        public void HandleDocusignNotification(string userId, string xmlPayload)
        {
            string envelopeId;

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException("userId");

            if (string.IsNullOrEmpty(xmlPayload))
                throw new ArgumentNullException("xmlPayload");

            try
            {
                envelopeId = GetEnvelopeIdFromXml(xmlPayload);
            }
            catch (ArgumentException)
            {
                string message = "Cannot extract envelopeId from DocuSign notification: UserId {0}, XML Payload\r\n{1}";
                Logger.GetLogger().WarnFormat(
                    message,
                    userId,
                    xmlPayload);
                throw new ArgumentException();
            }

            _alertReporter.ReportDocusignNotificationReceived(userId, xmlPayload);

            IEnumerable<ProcessDO> processList = GetProcessListForUser(userId);
            foreach (ProcessDO process in processList)
            {
                HandleIncomingNotification(userId, envelopeId, process);
            }
        }

        /// <summary>
        /// Returns the list of all processes to run for the specified user.
        /// </summary>
        public IEnumerable<ProcessDO> GetProcessListForUser(string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ProcessRepository.GetQuery().Where
                    (r => r.ProcessState == ProcessState.Unstarted
                        & r.UserId == userId).ToList();
            }
        }

        /// <summary>
        /// Handles a notification by DocuSign by an individual Process.
        /// </summary>
        /// <param name="userId">UserId received from DocuSign.</param>
        /// <param name="envelopeId">EnvelopeId received from DocuSign.</param>
        /// <param name="process">An instance of ProcessDO object for which processing should occur.</param>
        private void HandleIncomingNotification(string userId, string envelopeId, ProcessDO process)
        {
            _alertReporter.AlertProcessProcessing(userId, envelopeId, process.Id);
            //TODO: all notification processing logic.
        }

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
