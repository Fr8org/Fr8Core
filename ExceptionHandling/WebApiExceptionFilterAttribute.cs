using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using StructureMap;
using Hub.Exceptions;
using Hub.Managers;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;
using Fr8.Infrastructure;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;

namespace HubWeb.ExceptionHandling
{
    /// <summary>
    /// This exception filter handles any non-handled exception. Usually for such 
    /// exceptions we can provide no specific or useful instructions to user. 
    /// </summary>
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            ErrorDTO errorDto;
            // Collect error messages of all inner exceptions
            var alertManager = ObjectFactory.GetInstance<EventReporter>();
            var ex = context.Exception;
            //Post exception information to AppInsights
            Dictionary<string, string> properties = new Dictionary<string, string>();
            foreach (KeyValuePair<string, object> arg in context.ActionContext.ActionArguments)
            {
                properties.Add(arg.Key, JsonConvert.SerializeObject(arg.Value));
            }
            new TelemetryClient().TrackException(ex, properties);

            alertManager.UnhandledErrorCaught($"Unhandled exception has occurred.\r\nError message: {ex.GetFullExceptionMessage()}\r\nCall stack:\r\n{ex.StackTrace}");

            if (ex.GetType() == typeof(HttpException))
            {
                var httpException = (HttpException) ex;
                context.Response = new HttpResponseMessage((HttpStatusCode) httpException.GetHttpCode());
                context.Response = context.Request.CreateResponse(HttpStatusCode.Forbidden, ErrorDTO.AuthenticationError("Authorization has been denied for this request."));
                return;
            }
            if (ex.GetType() == typeof(MissingObjectException))
            {
                var missingObjectEx = (MissingObjectException) ex;
                context.Response = context.Request.CreateResponse(HttpStatusCode.BadRequest, ErrorDTO.InternalError(
                    missingObjectEx.Message,
                    "MISSING_OBJECT"));
                return;
            }
            if (ex.GetType() == typeof(WrongAuthenticationTypeException))
            {
                context.Response = context.Request.CreateResponse(HttpStatusCode.BadRequest,ErrorDTO.AuthenticationError("Terminal doesn't require authentication"));
                return;
            }
            context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            if (ex is AuthenticationExeception)
            {
                errorDto = ErrorDTO.AuthenticationError();
            }
            else
            {
                errorDto = ErrorDTO.InternalError();
            }

            errorDto.Message = "Sorry, an unexpected error has occurred while serving your request. Please try again in a few minutes.";
           
            // if debugging enabled send back the details of exception as well
            if (HttpContext.Current.IsDebuggingEnabled || string.Equals(CloudConfigurationManager.GetSetting("ForceExtendedDebuggingInfo"), "true", StringComparison.InvariantCultureIgnoreCase))
            {
                errorDto.Details = new {exception = context.Exception};
            }

            context.Response.Content = new StringContent(JsonConvert.SerializeObject(errorDto), Encoding.UTF8, "application/json");
        }
    }
}