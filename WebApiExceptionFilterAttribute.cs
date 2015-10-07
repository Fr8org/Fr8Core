using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;
using Core.Managers;
using Newtonsoft.Json;
using StructureMap;
using Utilities;
using PluginBase;

namespace Web
{
    /// <summary>
    /// This exception filter handles any non-handled exception. Usually for such 
    /// exceptions we can provide no specific or useful instructions to user. 
    /// </summary>
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            string errorMessage = "Sorry, an unexpected error has occurred while serving your request. Please try again in a few minutes.";
            
            var alertManager = ObjectFactory.GetInstance<EventReporter>();
            var ex = context.Exception;

            alertManager.UnhandledErrorCaught(
                String.Format("Unhandled exception has occurred.\r\nError message: {0}\r\nCall stack:\r\n{1}",
                ex.Message,
                ex.Source));

            context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            // if debugging enabled send back the details of exception as well
            if (HttpContext.Current.IsDebuggingEnabled)
            {
                if (ex is PluginCodedException) {
                    var pluginEx = (PluginCodedException)ex;
                    var pluginError = JsonConvert.SerializeObject(new { errorCode = pluginEx.ErrorCode, message = pluginEx.ErrorCode.GetEnumDescription() });
                    context.Response.Content = new StringContent(pluginError, Encoding.UTF8, "application/json");
                    return;
                }
                else {
                    var detailedMessage =
                        JsonConvert.SerializeObject(new { title = errorMessage, exception = context.Exception });

                    context.Response.Content = new StringContent(detailedMessage, Encoding.UTF8, "application/json");
                    return;
                }
            }
            context.Response.Content = new StringContent(errorMessage);
            
        }
    }
}