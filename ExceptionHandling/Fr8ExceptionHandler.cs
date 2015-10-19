using System.Web;
using System.Web.Http.ExceptionHandling;
using Data.Interfaces.DataTransferObjects;

namespace Web.ExceptionHandling
{
    public partial class Fr8ExceptionHandler : ExeceptionHandler
    {
        public override void HandleCore(ExceptionHandlerContext context)
        {
            var error = ErrorDTO.InternalError();

            error.Message = "Sorry, an unexpected error has occurred while serving your request. Please try again in a few minutes.";

            // if debugging enabled send back the details of exception as well
            if (HttpContext.Current.IsDebuggingEnabled)
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