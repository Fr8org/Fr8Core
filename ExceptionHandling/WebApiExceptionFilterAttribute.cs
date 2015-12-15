using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Hub.Exceptions;
using Hub.Managers;
using TerminalBase;
using Utilities;

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

            var alertManager = ObjectFactory.GetInstance<EventReporter>();
            var ex = context.Exception;

            alertManager.UnhandledErrorCaught(
                String.Format("Unhandled exception has occurred.\r\nError message: {0}\r\nCall stack:\r\n{1}",
                ex.Message,
                ex.StackTrace));

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
            if (HttpContext.Current.IsDebuggingEnabled)
            {
                if (ex is TerminalCodedException) 
                {
                    var terminalEx = (TerminalCodedException)ex;
                    
                    errorDto.Details = new
                    {
                        errorCode = terminalEx.ErrorCode, 
                        message = terminalEx.ErrorCode.GetEnumDescription()
                    };
                }
                else 
                {
                    errorDto.Details = new {exception = context.Exception};
                }
            }
            
            context.Response.Content = new StringContent(JsonConvert.SerializeObject(errorDto), Encoding.UTF8, "application/json");
        }
    }
}