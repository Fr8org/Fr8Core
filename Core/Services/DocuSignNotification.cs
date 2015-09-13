using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core.Interfaces;
using Core.Managers;
using Core.Utilities;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using StructureMap;
using Data.Interfaces.DataTransferObjects;

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

            List<DocuSignEventDO> curExternalEvents;
            string curEnvelopeId;
            Parse(xmlPayload, out curExternalEvents, out curEnvelopeId);
            ProcessEvents(curExternalEvents, userId);

            EventManager.DocuSignNotificationReceived();
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

        private void ProcessEvents(IEnumerable<DocuSignEventDO> curEvents, string curUserID)
        {
            ICrate _crate = ObjectFactory.GetInstance<ICrate>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var curEvent in curEvents)
                {
                    var crateFields = new List<FieldDTO>()
                    {
                        new FieldDTO () { Key = "EnvelopeId", Value = curEvent.EnvelopeId },
                        new FieldDTO() { Key = "ExternalEventType", Value = curEvent.ExternalEventType.ToString() },
                        new FieldDTO() {Key = "RecipientId", Value = curEvent.RecipientId.ToString() }
                    };
                    var curEventData = _crate.Create("Event Data", Newtonsoft.Json.JsonConvert.SerializeObject(crateFields));
                    //load a list of all of the ProcessTemplateDO that have subscribed to this particular DocuSign event
                    var subscriptions = 
                        uow.ExternalEventSubscriptionRepository.GetQuery().Include(p => p.ExternalProcessTemplate)
                            .Where(s => s.ExternalEvent == curEvent.ExternalEventType && s.ExternalProcessTemplate.DockyardAccount.Id == curUserID)
                            .ToList();

                    foreach (var subscription in subscriptions)
                    {
                        //checkpoint: figure out why the processnode is not the "50" one that was configured
                        _processTemplate.LaunchProcess(uow, subscription.ExternalProcessTemplate, curEventData);
                    }
                }
                
                uow.SaveChanges();
            }
        }
    }
}