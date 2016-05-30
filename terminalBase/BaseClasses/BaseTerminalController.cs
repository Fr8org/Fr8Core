using System;
using System.Threading.Tasks;
using System.Web.Http;
using TerminalBase.Infrastructure;
using Utilities.Logging;
using Utilities;

namespace TerminalBase.BaseClasses
{
    //this is a quasi base class. We can't use inheritance directly because it's across project boundaries, but
    //we can generate instances of this.
    public class BaseTerminalController : ApiController
    {
        private readonly BaseTerminalEvent _baseTerminalEvent;

        public BaseTerminalController()
        {
            _baseTerminalEvent = new BaseTerminalEvent();
        }

        /// <summary>
        /// Reports Terminal Error incident
        /// </summary>
        [HttpGet]
        public IHttpActionResult ReportTerminalError(string terminalName, Exception terminalError, string userId = null)
        {
            var exceptionMessage = terminalError.GetFullExceptionMessage() + "  \n\r   " + terminalError.ToString();//string.Format("{0}\r\n{1}", terminalError.Message, terminalError.StackTrace);

            try
            {
                return Json(_baseTerminalEvent.SendTerminalErrorIncident(terminalName, exceptionMessage, terminalError.GetType().Name, userId));
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error has occurred in terminal [{terminalName}]. {exceptionMessage} | Fr8UserId = {userId} \n\r "
                                    + $"Additionally, an error has occurred while trying to post error details to the Hub. {ex.Message}";

                Logger.LogError(errorMessage);
                //Logger.GetLogger().ErrorFormat(errorMessage, terminalName, exceptionMessage, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Reports start up incident
        /// </summary>
        /// <param name="terminalName">Name of the terminal which is starting up</param>
        [HttpGet]
        public IHttpActionResult AfterStartup(string terminalName)
        {
            return null;

            //TODO: Commented during development only. So that app loads fast.
            return Json(ReportStartUp(terminalName));
        }

        /// <summary>
        /// Reports start up event by making a Post request
        /// </summary>
        /// <param name="terminalName"></param>
        private Task<string> ReportStartUp(string terminalName)
        {
            return _baseTerminalEvent.SendEventOrIncidentReport(terminalName, "Terminal Fact");
        }

   
    }
}
