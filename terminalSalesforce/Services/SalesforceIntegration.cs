using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using terminalSalesforce.Infrastructure;
using Utilities.Logging;
using Data.Entities;

namespace terminalSalesforce.Services
{
    public class SalesforceIntegration : ISalesforceIntegration
    {
        private Authentication _authentication = new Authentication();
        private Lead _lead = new Lead();
        private Contact _contact = new Contact();
        private Account _account = new Account();
        public SalesforceIntegration()
        {
           
        }

        public bool CreateLead(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            bool createFlag = true;
            try
            {
                var authTokenResult = Task.Run(() => _authentication.RefreshAccessToken(authTokenDO)).Result;
                var createtask = _lead.CreateLead(actionDO, authTokenResult);
            }
            catch (Exception ex)
            {
                createFlag = false;
                Logger.GetLogger().Error(ex);
                throw;
            }
            return createFlag;
        }

        public bool CreateContact(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            bool createFlag = true;
            try
            {
                var authTokenResult = Task.Run(() => _authentication.RefreshAccessToken(authTokenDO)).Result;

                var createtask = _contact.CreateContact(actionDO, authTokenDO);
            }
            catch (Exception ex)
            {
                createFlag = false;
                Logger.GetLogger().Error(ex);
                throw;
            }
            return createFlag;
        }

        public bool CreateAccount(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            bool createFlag = true;
            try
            {
                var authTokenResult = Task.Run(() => _authentication.RefreshAccessToken(authTokenDO)).Result;

                var createtask = _account.CreateAccount(actionDO, authTokenResult);
            }
            catch (Exception ex)
            {
                createFlag = false;
                Logger.GetLogger().Error(ex);
                throw;
            }
            return createFlag;
        }

        public async Task<IList<FieldDTO>> GetFieldsList(ActionDO actionDO, AuthorizationTokenDO authTokenDO, string salesforceObjectName)
        {
            try
            {
                switch (salesforceObjectName)
                {
                    case "Account":
                        return await _account.GetAccountFields(actionDO, authTokenDO);
                    case "Lead":
                        return await _lead.GetLeadFields(actionDO, authTokenDO);
                    case "Contact":
                        return await _contact.GetContactFields(actionDO, authTokenDO);
                    default:
                        throw new NotSupportedException(
                            string.Format("Not Supported Salesforce object name {0} has been given for querying fields.",
                                salesforceObjectName));
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error(ex);
                throw;
            }
        }

        public async Task<StandardPayloadDataCM> GetObject(ActionDO actionDO, AuthorizationTokenDO authTokenDO, string salesforceObjectName, string condition)
        {
            try
            {
                var payloadObjectDTO = new StandardPayloadDataCM();
                        
                switch (salesforceObjectName)
                {
                    case "Account":
                        payloadObjectDTO.ObjectType = "Salesforce Accounts";
                        var resultAccounts = await _account.GetAccounts(actionDO, authTokenDO, condition);

                        payloadObjectDTO.PayloadObjects.AddRange(
                            resultAccounts.ToList().Select(account => new PayloadObjectDTO
                            {
                                PayloadObject = new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "AccountNumber", Value = account.AccountNumber},
                                    new FieldDTO {Key = "Name", Value = account.Name},
                                    new FieldDTO {Key = "Phone", Value = account.Phone}
                                }
                            }));

                        return payloadObjectDTO;
                    case "Lead":
                        payloadObjectDTO.ObjectType = "Salesforce Leads";
                        var resultLeads = await _lead.GetLeads(actionDO, authTokenDO, condition);

                        payloadObjectDTO.PayloadObjects.AddRange(
                            resultLeads.ToList().Select(lead => new PayloadObjectDTO
                            {
                                PayloadObject = new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "Id", Value = lead.Id},
                                    new FieldDTO {Key = "FirstName", Value = lead.FirstName},
                                    new FieldDTO {Key = "LastName", Value = lead.LastName},
                                    new FieldDTO {Key = "Company", Value = lead.Company},
                                    new FieldDTO {Key = "Title", Value = lead.Title}
                                }
                            }));

                        return payloadObjectDTO;
                    case "Contact":
                        payloadObjectDTO.ObjectType = "Salesforce Contacts";
                        var resultContacts = await _contact.GetContacts(actionDO, authTokenDO, condition);

                        payloadObjectDTO.PayloadObjects.AddRange(
                            resultContacts.ToList().Select(contact => new PayloadObjectDTO
                            {
                                PayloadObject = new List<FieldDTO>
                                {
                                    new FieldDTO {Key = "FirstName", Value = contact.FirstName},
                                    new FieldDTO {Key = "LastName", Value = contact.LastName},
                                    new FieldDTO {Key = "MobilePhone", Value = contact.MobilePhone},
                                    new FieldDTO {Key = "Email", Value = contact.Email}
                                }
                            }));

                        return payloadObjectDTO;
                    default:
                        throw new NotSupportedException(
                            string.Format("Not Supported Salesforce object name {0} has been given for querying fields.",
                                salesforceObjectName));
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error(ex);
                throw;
            }
        }


    }
}