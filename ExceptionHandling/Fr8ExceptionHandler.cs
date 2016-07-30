using System;
using System.Web;
using System.Web.Http.ExceptionHandling;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;

namespace HubWeb.ExceptionHandling
{
    public partial class Fr8ExceptionHandler : ExeceptionHandler
    {
        public override void HandleCore(ExceptionHandlerContext context)
        {
            //Self-host fix
            if (HttpContext.Current == null) return;

            var error = ErrorDTO.InternalError();

            error.Message = "Sorry, an unexpected error has occurred while serving your request. Please try again in a few minutes.";

            // if debugging enabled send back the details of exception as well
            if (HttpContext.Current.IsDebuggingEnabled || string.Equals(CloudConfigurationManager.GetSetting("ForceExtendedDebuggingInfo"), "true", StringComparison.InvariantCultureIgnoreCase))
            {
                error.Details = new
                {
                    exception = context.Exception,
                    catchBlock = context.CatchBlock,
                };
            }
            
            context.Result = new ErrorResult(context.ExceptionContext.Request, error);
        }
    }
}