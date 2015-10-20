using Data.Interfaces.DataTransferObjects;
using pluginSalesforce.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using StructureMap;
using Utilities.Logging;
using Salesforce.Force;
using System.Threading.Tasks;
using Salesforce.Common;
using Microsoft.WindowsAzure;
using System.Net.Http;
using Newtonsoft.Json;
using Data.Interfaces.ManifestSchemas;
using Salesforce.Common.Models;
using Data.Interfaces;

namespace pluginSalesforce.Services
{
    public class SalesforceIntegration : ISalesforceIntegration
    {
        private Authentication _authentication = new pluginSalesforce.Infrastructure.Authentication();
        private Lead _lead = new Lead();

        public SalesforceIntegration()
        {
           
        }

        public bool CreateLead(ActionDTO currentDTO)
        {
            bool createFlag = true;
            try
            {
                var actionDTO = Task.Run(() => _authentication.RefreshAccessToken(currentDTO)).Result;
                currentDTO = actionDTO;
                var createtask = _lead.CreateLead(currentDTO);
            }
            catch (Exception ex)
            {
                createFlag = false;
                Logger.GetLogger().Error(ex);
                throw;
            }
            return createFlag;
        }         
    }
}