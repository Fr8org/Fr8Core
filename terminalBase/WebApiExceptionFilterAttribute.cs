using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using StructureMap;
using Hub.Managers;
using TerminalBase.BaseClasses;
using Utilities;

namespace TerminalBase
{
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            //get the terminal error details
            var curTerminalError = actionExecutedContext.Exception;
            var terminalName = GetTerminalName(actionExecutedContext.ActionContext.ControllerContext.Controller);

            //POST event to fr8 about this terminal error
            new BaseTerminalController().ReportTerminalError(terminalName, curTerminalError);

            //prepare the response JSON based on the exception type
            actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            if (curTerminalError is TerminalCodedException)
            {
                //if terminal error is terminal Coded Exception, place the error code description in message
                var terminalEx = (TerminalCodedException)curTerminalError;
                var terminalError =
                    JsonConvert.SerializeObject(new {status = "terminal_error", message = terminalEx.ErrorCode.GetEnumDescription()});
                actionExecutedContext.Response.Content = new StringContent(terminalError, Encoding.UTF8, "application/json");
            }
            else
            {
                //if terminal error is general exception, place expcetion message
                var detailedMessage =
                    JsonConvert.SerializeObject(new { status = "terminal_error", message = curTerminalError.Message });
                actionExecutedContext.Response.Content = new StringContent(detailedMessage, Encoding.UTF8, "application/json");
            }
        }

        private string GetTerminalName(IHttpController curController)
        {
            string assemblyName = curController.GetType().Assembly.FullName.Split(new char[] {','})[0];
            return assemblyName;
        }
    }
}
