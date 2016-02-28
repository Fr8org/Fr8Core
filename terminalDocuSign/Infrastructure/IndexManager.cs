using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.Manifests;
using Data.Repositories;
using Hub.Services;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;

namespace terminalDocuSign.Infrastructure
{
    public class IndexManager
    {
        public AuthorizationTokenDO currentAuthToken;
        public void IndexDocuSignUser(AuthorizationTokenDO curAuthToken, DateTime startDate)
        {
            try
            {
                currentAuthToken = curAuthToken;
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {

                    var parsingRecords = uow.MultiTenantObjectRepository.Get<StandardParsingRecordCM>(uow, curAuthToken.UserID, a => a.ExternalAccountId == curAuthToken.ExternalAccountId);
                    if (parsingRecords != null)
                    {
                        //Get find period to Query 
                        if (DateTime.Parse(parsingRecords.EndDate) < startDate)
                        {
                            QueryParse(startDate, DateTime.Now);
                        }
                        else if (DateTime.Parse(parsingRecords.EndDate) > startDate)
                        {
                            QueryParse(DateTime.Parse(parsingRecords.EndDate), DateTime.Now);

                            //To check for Gaps
                            if(startDate<DateTime.Parse(parsingRecords.StartDate))
                            {
                                QueryParse(startDate, DateTime.Now);
                            }
                        }
                    }
                    else
                    {
                        QueryParse(startDate, DateTime.Now);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void QueryParse(DateTime startDate, DateTime endDate)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(currentAuthToken.Token);
            int resultCount = 100;

            DocuSignEnvelope docusignEnvelope = new DocuSignEnvelope(
                docuSignAuthDTO.Email,
                docuSignAuthDTO.ApiPassword);           
          
                var accountEnvelopes = docusignEnvelope.GetEnvelopes(startDate, endDate, true, resultCount);
                ProcessEnvelope(accountEnvelopes);
                ProcessRecipients(accountEnvelopes);
                AddorUpdateParsingRecord(accountEnvelopes, startDate);
           
        }

        public void ProcessEnvelope(AccountEnvelopes accountEnvelopes)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var envelope in accountEnvelopes.Envelopes)
                {
                    var savedEnvelope = uow.MultiTenantObjectRepository.Get<DocuSignEnvelopeCM>(uow, currentAuthToken.UserID, a => a.EnvelopeId == envelope.EnvelopeId);
                    if (savedEnvelope == null)
                    {
                        DocuSignEnvelopeCM docuSignEnvelope = new DocuSignEnvelopeCM
                        {
                            CompletedDate = DateTimeHelper.Parse(envelope.CompletedDate),
                            CreateDate = DateTimeHelper.Parse(envelope.CreateDate),
                            EnvelopeId = envelope.EnvelopeId,
                            ExternalAccountId = currentAuthToken.ExternalAccountId,
                            Status = envelope.Status,
                            StatusChangedDateTime= DateTimeHelper.Parse(envelope.StatusChangedDateTime)
                        };
                        uow.MultiTenantObjectRepository.AddOrUpdate<DocuSignEnvelopeCM>(uow, currentAuthToken.UserID, docuSignEnvelope, e => e.EnvelopeId);
                        uow.SaveChanges();
                    }
                    else
                    {
                        var envelopeStatusChangedTime = DateTimeHelper.Parse(envelope.StatusChangedDateTime);

                        if (savedEnvelope.StatusChangedDateTime == null || envelopeStatusChangedTime > savedEnvelope.StatusChangedDateTime)
                        {
                            DocuSignEnvelopeCM docuSignEnvelope = new DocuSignEnvelopeCM
                            {
                                CompletedDate = DateTimeHelper.Parse(envelope.CompletedDate),
                                CreateDate = DateTimeHelper.Parse(envelope.CreateDate),
                                EnvelopeId = envelope.EnvelopeId,
                                ExternalAccountId = currentAuthToken.ExternalAccountId,
                                Status = envelope.Status,
                                StatusChangedDateTime = DateTimeHelper.Parse(envelope.StatusChangedDateTime)
                            };
                            uow.MultiTenantObjectRepository.AddOrUpdate<DocuSignEnvelopeCM>(uow, currentAuthToken.UserID, docuSignEnvelope, e => e.EnvelopeId);
                            uow.SaveChanges();
                        }
                    }
                }
            }
        }

        public void ProcessRecipients(AccountEnvelopes accountEnvelopes)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var recipient in accountEnvelopes.Recipients)
                {
                   
                        DocuSignRecipientCM docuSignRecipient = new DocuSignRecipientCM
                        {
                            Object = recipient.Object,
                            DocuSignAccountId = recipient.DocuSignAccountId,
                            Status = recipient.Status,
                            RecipientId = recipient.RecipientId
                        };
                        uow.MultiTenantObjectRepository.AddOrUpdate<DocuSignRecipientCM>(uow, currentAuthToken.UserID, docuSignRecipient, e => e.RecipientId);                        
                        uow.SaveChanges();
                   
                }
            }
        }

        public void AddorUpdateParsingRecord(AccountEnvelopes accountEnvelopes, DateTime startDate)
        {
            StandardParsingRecordCM parsingRecord = new StandardParsingRecordCM
            {
                Name = "Standard Parsing Record",
                ExternalAccountId = currentAuthToken.ExternalAccountId,
                InternalAccountId = currentAuthToken.UserID,
                EndDate = DateTime.Now.ToString(),
                StartDate = startDate.ToString(),
                Service = "DocuSign Service"
            };
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.MultiTenantObjectRepository.AddOrUpdate<StandardParsingRecordCM>(uow, currentAuthToken.UserID, parsingRecord, a => a.ExternalAccountId);
                uow.SaveChanges();
            }
        }
    }
}