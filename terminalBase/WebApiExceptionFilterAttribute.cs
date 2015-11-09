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
            //get the plugin error details
            var curPluginError = actionExecutedContext.Exception;
            var pluginName = GetTerminalName(actionExecutedContext.ActionContext.ControllerContext.Controller);

            //POST event to fr8 about this plugin error
            new BaseTerminalController().ReportTerminalError(pluginName, curPluginError);

            //prepare the response JSON based on the exception type
            actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            if (curPluginError is TerminalCodedException)
            {
                //if plugin error is Plugin Coded Exception, place the error code description in message
                var pluginEx = (TerminalCodedException) curPluginError;
                var pluginError =
                    JsonConvert.SerializeObject(new {status = "plugin_error", message = pluginEx.ErrorCode.GetEnumDescription()});
                actionExecutedContext.Response.Content = new StringContent(pluginError, Encoding.UTF8, "application/json");
            }
            else
            {
                //if plugin error is general exception, place expcetion message
                var detailedMessage =
                    JsonConvert.SerializeObject(new { status = "plugin_error", message = curPluginError.Message });
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
