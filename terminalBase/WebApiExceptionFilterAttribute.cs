using System;
using System.Web.Http.Controllers;
using Core.Managers;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Filters;
using TerminalBase.BaseClasses;
using StructureMap;
using Utilities;

namespace TerminalBase
{
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            //get the plugin error details
            var curPluginError = actionExecutedContext.Exception;
            var pluginName = GetPluginName(actionExecutedContext.ActionContext.ControllerContext.Controller);

            //POST event to fr8 about this plugin error
            new BasePluginController().ReportPluginError(pluginName, curPluginError);

            //prepare the response JSON based on the exception type
            actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            if (curPluginError is PluginCodedException)
            {
                //if plugin error is Plugin Coded Exception, place the error code description in message
                var pluginEx = (PluginCodedException) curPluginError;
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

        private string GetPluginName(IHttpController curController)
        {
            string assemblyName = curController.GetType().Assembly.FullName.Split(new char[] {','})[0];
            return assemblyName;
        }
    }
}
