using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Errors;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;

namespace Fr8.TerminalBase.Filters
{
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            //if (actionExecutedContext.Exception is TaskCanceledException)
            //{
            //    // TaskCanceledException is an exception representing a successful task cancellation 
            //    // Don't need to log it
            //    // Ref: https://msdn.microsoft.com/en-us/library/dd997396(v=vs.110).aspx
            //    return;
            //}

            //get the terminal error details
            var curTerminalError = actionExecutedContext.Exception;
            var curController = actionExecutedContext.ActionContext.ControllerContext.Controller;
            var terminalName = GetTerminalName(curController);
            
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

            if (curTerminalError is AuthorizationTokenExpiredOrInvalidException)
            {
                statusCode = (HttpStatusCode)419;
            }
            else if (curTerminalError is AggregateException)
            {
                foreach (var innerEx in ((AggregateException)curTerminalError).InnerExceptions)
                {
                    if (innerEx is AuthorizationTokenExpiredOrInvalidException)
                    {
                        statusCode = (HttpStatusCode)419;
                    }
                }
            }

            


            //Post exception information to AppInsights
            Dictionary<string, string> properties = new Dictionary<string, string>();
            foreach (KeyValuePair<string, object> arg in actionExecutedContext.ActionContext.ActionArguments)
            {
                properties.Add(arg.Key, JsonConvert.SerializeObject(arg.Value));
            }
            properties.Add("Terminal", terminalName);
            new TelemetryClient().TrackException(curTerminalError, properties);

            string userId = null;
            if(!string.IsNullOrEmpty(actionExecutedContext.ActionContext.ControllerContext.RequestContext.Principal?.Identity?.AuthenticationType))
            {
                userId = actionExecutedContext.ActionContext.ControllerContext.RequestContext.Principal.Identity.AuthenticationType;
            }

            //Log exception
            Logger.GetLogger(terminalName).Error($"Current controller: [{curController}]. Fr8UserId: [{userId}]. Exception: [{curTerminalError.GetFullExceptionMessage()}]");

            //POST event to fr8 about this terminal error
            //new BaseTerminalController().ReportTerminalError(terminalName, curTerminalError,userId);


            //prepare the response JSON based on the exception type
            actionExecutedContext.Response = new HttpResponseMessage(statusCode);
            if (curTerminalError is TerminalCodedException)
            {
                //if terminal error is terminal Coded Exception, place the error code description in message
                var terminalEx = (TerminalCodedException)curTerminalError;
                var terminalError =
                    JsonConvert.SerializeObject(new { status = "terminal_error", message = terminalEx.ErrorCode.GetEnumDescription() });
                actionExecutedContext.Response.Content = new StringContent(terminalError, Encoding.UTF8, "application/json");
            }
            else
            {
                var detailedMessage =
                    JsonConvert.SerializeObject(new { status = "terminal_error", message = curTerminalError.Message });
                actionExecutedContext.Response.Content = new StringContent(detailedMessage, Encoding.UTF8, "application/json");
            }
        }

        private string GetTerminalName(IHttpController curController)
        {
            string assemblyName = curController.GetType().Assembly.FullName.Split(new char[] { ',' })[0];
            return assemblyName;
        }
    }
}
