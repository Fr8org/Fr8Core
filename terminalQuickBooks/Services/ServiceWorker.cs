using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Storage.Basic;
using Intuit.Ipp.Core;
using Intuit.Ipp.Core.Configuration;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Diagnostics;
using Intuit.Ipp.Security;
using terminalQuickBooks.Infrastructure;
using terminalQuickBooks.Interfaces;
using Utilities.Configuration.Azure;


namespace terminalQuickBooks.Services
{
    public class ServiceWorker : IServiceWorker
    {
        private static readonly string AppToken =
            CloudConfigurationManager.GetSetting("QuickBooksAppToken").ToString(CultureInfo.InvariantCulture);
      
        public ServiceContext CreateServiceContext(string accessToken)
        {
            var tokens = accessToken.Split(new[] {Authenticator.TokenSeparator}, StringSplitOptions.None);
            var accToken = tokens[0];
            var accTokenSecret = tokens[1];
            var companyId = tokens[2];
            var oauthValidator = new OAuthRequestValidator(
                accToken, 
                accTokenSecret, 
                Authenticator.ConsumerKey, 
                Authenticator.ConsumerSecret);
            return new ServiceContext(AppToken, companyId, IntuitServicesType.QBO, oauthValidator);
        }

        public DataService GetDataService(AuthorizationTokenDO authTokenDO)
        {
            var curServiceContext = CreateServiceContext(authTokenDO.Token);
            //Modify required settings for the Service Context
            curServiceContext.IppConfiguration.BaseUrl.Qbo = "https://sandbox-quickbooks.api.intuit.com/";
            curServiceContext.IppConfiguration.MinorVersion.Qbo = "4";
            curServiceContext.IppConfiguration.Logger.RequestLog.EnableRequestResponseLogging = true;
            curServiceContext.IppConfiguration.Logger.CustomLogger = new TraceLogger();
            curServiceContext.IppConfiguration.Message.Response.SerializationFormat = SerializationFormat.Json;

            var curDataService = new DataService(curServiceContext);
            curServiceContext.UseDataServices();
            return curDataService;
        }
    }
}