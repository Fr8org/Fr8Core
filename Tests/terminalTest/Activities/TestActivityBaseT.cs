using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using TerminalBase.BaseClasses;

namespace terminalTest.Actions
{
    public abstract class TestActivityBase<T> : EnhancedTerminalActivity<T>
        where T: StandardConfigurationControlsCM
    {
        protected TestActivityBase() 
            : base(false)
        {
        }

        /// <summary>
        /// DON'T USE THIS FUNCTION THIS IS JUST FOR BACKWARD COMPABILITY !!
        /// </summary>
        /// <param name="activityTemplateName"></param>
        /// <returns></returns>
        [Obsolete("This function is for backward comatibility only. Please use Task<ActivityTemplateDTO> GetActivityTemplate(string, string, string, string)")]
        protected async Task<ActivityTemplateDTO> GetActivityTemplateByName(string activityTemplateName)
        {
            var allActivityTemplates = _activityTemplateCache ?? (_activityTemplateCache = await HubCommunicator.GetActivityTemplates(CurrentUserId));
            var foundActivity = allActivityTemplates.FirstOrDefault(a => a.Name == activityTemplateName);

            if (foundActivity == null)
            {
                throw new Exception($"ActivityTemplate was not found. ActivitiyTemplateName: {activityTemplateName}");
            }

            return foundActivity;
        }
        protected void Log(string message)
        {
            // use any logging logic you want
            //File.AppendAllText(@"C:\Work\fr8_research\log.txt", message + "\n");
        }
    }
}