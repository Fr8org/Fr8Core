using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
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


    }
}