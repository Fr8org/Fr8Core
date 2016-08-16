using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Fr8.Infrastructure.Utilities;
using FluentValidation.WebApi;
using Hub.Infrastructure;
using Hub.ModelBinders;
using HubWeb.App_Start;
using HubWeb.ExceptionHandling;
using LogentriesCore.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Segment;
using Microsoft.ApplicationInsights.Extensibility;
using Logger = Fr8.Infrastructure.Utilities.Logging.Logger;
using System.Globalization;
using Hub.Interfaces;
using StructureMap;

namespace HubWeb
{
    public class MvcApplication : HttpApplication
    {
        private static bool _IsInitialised;
        private const string AngularRootPath = "/dashboard";

        protected void Application_Start()
        {
            if (CultureInfo.CurrentCulture.Parent.LCID != 9)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(1033);
            }
            Init(false);
        }

        public void Init(bool selfHostMode = false)
        {
            if (!selfHostMode)
            {
                GlobalConfiguration.Configure(WebApiConfig.Register);
                AreaRegistration.RegisterAllAreas();
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);
            }

            // Configure formatters
            // Enable camelCasing in JSON responses
            var formatters = GlobalConfiguration.Configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            //Register global Exception Filter for WebAPI 
            GlobalConfiguration.Configuration.Filters.Add(new WebApiExceptionFilterAttribute());

            if (!selfHostMode)
            {
                Fr8.Infrastructure.Utilities.Server.ServerPhysicalPath = Server.MapPath("~");
                var segmentWriteKey =
                    Fr8.Infrastructure.Utilities.Configuration.CloudConfigurationManager.GetSetting("SegmentWriteKey");
                if (!segmentWriteKey.IsNullOrEmpty())
                    Analytics.Initialize(segmentWriteKey);
            }

            ModelBinders.Binders.Add(typeof(DateTimeOffset), new KwasantDateBinder());

            Logger.GetLogger().Warn("Fr8 starting...");

            ConfigureValidationEngine();
        }

        private void ConfigureValidationEngine()
        {
            FluentValidationModelValidatorProvider.Configure(GlobalConfiguration.Configuration);
        }


        protected void Application_Error(Object sender, EventArgs e)
        {
            var exception = Server.GetLastError();

            String errorMessage = "Critical internal error occured.";
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                    errorMessage += " URL accessed: " + HttpContext.Current.Request.Url;
            }
            catch (Exception)
            {
                errorMessage += " Error on startup.";
            }


            //Logger.GetLogger().Error(errorMessage, exception);
            Logger.GetLogger().Error($"{exception}");
        }

        //Optimization. Even if running in DEBUG mode, this will only execute once.
        //But on production, there is no need for this call
        protected void Application_BeginRequest(object sender, EventArgs e)
        {

#if DEBUG
            //SetServerUrl(HttpContext.Current);
            TelemetryConfiguration.Active.DisableTelemetry = true;
#endif
            NormalizeUrl();
            RewriteAngularRequests();
        }

        private void RewriteAngularRequests()
        {
            if (Request.Url.LocalPath.StartsWith(AngularRootPath))
                Context.RewritePath(AngularRootPath);
        }

        /// <summary>
        /// Make sure that User is accessing the website using correct and secure URL
        /// </summary>
        private void NormalizeUrl()
        {
            // Ignore requests to dev and API since API clients usually cannot process 301 redirects
            if (Request.Url.PathAndQuery.ToLower().StartsWith("/api")
                || Request.Url.PathAndQuery.ToLower().StartsWith("/authenticationcallback")
                || Request.Url.Host.ToLower().Contains("dev"))
                return;

            // Force user to fr8.co from fr8.company (old address)
            if (Request.Url.Host.Contains("fr8.company") || Request.Url.Host.StartsWith("www."))
            {
                RedirectToCanonicalUrl();
            }

            // Force user to http if user is accessing the PROD site
            if (Request.Url.Host.StartsWith("fr8.co"))
            {
                switch (Request.Url.Scheme)
                {
                    case "https":
                        Response.AddHeader("Strict-Transport-Security", "max-age=300");
                        break;
                    case "http":
                        RedirectToCanonicalUrl();
                        break;
                }
            }
        }

        private void RedirectToCanonicalUrl()
        {
            var path = "https://fr8.co" + Request.Url.PathAndQuery;
            Response.Status = "301 Moved Permanently";
            Response.AddHeader("Location", path);
        }

        /*private void SetServerUrl(HttpContext context = null)
        {
            if (!_IsInitialised)
            {
                lock (_initLocker)
                {
                    //Not redunant - this check is more efficient for a 1-time set.
                    //If it's set, we exit without locking. We want to avoid locking as much as possible, so only do it once (at startup)
                    if (!_IsInitialised)
                    {
                        //First, try to read from the config
                        var config = ObjectFactory.GetInstance<IConfigRepository>();
                        var serverProtocol = config.Get("ServerProtocol", String.Empty);
                        var domainName = config.Get("ServerDomainName", String.Empty);
                        var domainPort = config.Get<int?>("ServerPort", null);

                        if (!String.IsNullOrWhiteSpace(domainName) && !String.IsNullOrWhiteSpace(serverProtocol) && domainPort.HasValue)
                        {
                            Fr8.Infrastructure.Utilities.Server.ServerUrl = String.Format("{0}{1}{2}/", serverProtocol, domainName,
                                domainPort.Value == 80 ? String.Empty : (":" + domainPort.Value));

                            Fr8.Infrastructure.Utilities.Server.ServerHostName = domainName;
                        }
                        else
                        {
                            if (context == null)
                                return;

                            //If the config is not set, then we setup our server URL based on the first request
                            string port = context.Request.ServerVariables["SERVER_PORT"];
                            if (port == null || port == "80" || port == "443")
                                port = "";
                            else
                                port = ":" + port;

                            string protocol = context.Request.ServerVariables["SERVER_PORT_SECURE"];
                            if (protocol == null || protocol == "0")
                                protocol = "http://";
                            else
                                protocol = "https://";

                            // *** Figure out the base Url which points at the application's root
                            Fr8.Infrastructure.Utilities.Server.ServerHostName = context.Request.ServerVariables["SERVER_NAME"];
                            string url = protocol + context.Request.ServerVariables["SERVER_NAME"] + port + context.Request.ApplicationPath;
                            Fr8.Infrastructure.Utilities.Server.ServerUrl = url;
                        }
                        _IsInitialised = true;
                    }
                }
            }
        }*/

        public void Application_End()
        {
            //Logger.GetLogger().Info("fr8 web shutting down...");
            Logger.GetLogger().Warn("fr8 web shutting down...");

            // This will give LE background thread some time to finish sending messages to Logentries.
            var numWaits = 3;
            while (!AsyncLogger.AreAllQueuesEmpty(TimeSpan.FromSeconds(5)) && numWaits > 0)
                numWaits--;
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var principal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            if (principal != null)
            {
                var claims = principal.Claims;
                var roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
                var userPrincipal = new Fr8Principal(null, principal.Identity, roles);
                /*
                GenericPrincipal userPrincipal = new GenericPrincipal(principal.Identity,
                                         claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray());
                */
                Context.User = userPrincipal;
            }
        }
    }
}

