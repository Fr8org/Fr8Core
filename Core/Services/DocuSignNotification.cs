using System;
using System.Collections.Generic;
using System.Linq;
using Core.Interfaces;
using Core.Managers;
using Core.Utilities;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace Core.Services
{
    public class DocuSignNotification : IDocuSignNotification
    {
        private readonly EventReporter _alertReporter;
        private readonly IProcessTemplate _processTemplate;

        public DocuSignNotification(EventReporter alertReporter, DockyardAccount userService)
        {
            _alertReporter = alertReporter;
            _processTemplate = ObjectFactory.GetInstance<ProcessTemplate>();
        }

        /// <summary>
        /// The method processes incoming notifications from DocuSign. 
        /// </summary>
        /// <param name="userId">UserId received from DocuSign.</param>
        /// <param name="xmlPayload">XML content received from DocuSign.</param>
        public void Process(string userId, string xmlPayload)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException("userId");

            if (string.IsNullOrEmpty(xmlPayload))
                throw new ArgumentNullException("xmlPayload");

            List<DocuSignEventDO> curEvents;
            string curEnvelopeId;
            Parse(xmlPayload, out curEvents, out curEnvelopeId);
            ProcessEvents(curEvents);

            EventManager.DocuSignNotificationReceived(curEnvelopeId);
        }

        private void Parse(string xmlPayload, out List<DocuSignEventDO> curEvents, out string curEnvelopeId)
        {
            curEvents = new List<DocuSignEventDO>();
            try
            {
                var docuSignEnvelopeInformation = DocuSignConnectParser.GetEnvelopeInformation(xmlPayload);
                curEnvelopeId = docuSignEnvelopeInformation.EnvelopeStatus.EnvelopeId;
                curEvents.Add(new DocuSignEventDO
                {
                    ExternalEventType =
                        ExternalEventType.MapEnvelopeExternalEventType(docuSignEnvelopeInformation.EnvelopeStatus.Status),
                    EnvelopeId = docuSignEnvelopeInformation.EnvelopeStatus.EnvelopeId
                });
            }
            catch (ArgumentException)
            {
                _alertReporter.ImproperDocusignNotificationReceived(
                    "Cannot extract envelopeId from DocuSign notification: UserId {0}, XML Payload\r\n{1}");
                throw new ArgumentException();
            }
        }

        private void ProcessEvents(IEnumerable<DocuSignEventDO> curEvents)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var curEvent in curEvents)
                {
                    var @event = curEvent;
                    var subscriptions =
                        uow.ExternalEventRegistrationRepository.GetQuery()
                            .Where(s => s.EventType == @event.ExternalEventType)
                            .ToList();
                    var curEnvelope = uow.EnvelopeRepository.GetByKey(curEvent.Id);

                    foreach (var subscription in subscriptions)
                    {
                        _processTemplate.LaunchProcess(subscription.ProcessTemplateId, curEnvelope);
                    }
                }
            }
        }
    }
}