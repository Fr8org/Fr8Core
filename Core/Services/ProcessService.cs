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
    public class ProcessService : IProcessService
    {
        EventReporter _alertReporter;
        DockyardAccount _userService;
        IDocusignXml _docusignXml;

        public ProcessService(EventReporter alertReporter, DockyardAccount userService, IDocusignXml docusignXml)
        {
            _alertReporter = alertReporter;
            _userService = userService;
            _docusignXml = docusignXml;
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
                envelopeId = _docusignXml.GetEnvelopeIdFromXml(xmlPayload);
            }
            catch (ArgumentException)
            {
                string message = "Cannot extract envelopeId from DocuSign notification: UserId {0}, XML Payload\r\n{1}";
                _alertReporter.ImproperDocusignNotificationReceived(message);
                throw new ArgumentException();
            }

            _alertReporter.DocusignNotificationReceived(userId, xmlPayload);

            IEnumerable<ProcessDO> processList = _userService.GetProcessList(userId);
            foreach (ProcessDO process in processList)
            {
                HandleIncomingNotification(userId, envelopeId, process);
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

    }
}
