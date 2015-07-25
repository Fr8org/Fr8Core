using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using Core.Managers;
using StructureMap;

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
            context.Response.Content = new StringContent(errorMessage);
        }
    }
}