using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalTest.Actions
{
    public abstract class TestActivityBase<T> : EnhancedTerminalActivity<T>
        where T: StandardConfigurationControlsCM
    {
        protected TestActivityBase(ICrateManager crateManager) 
            : base(false, crateManager)
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
            var allActivityTemplates = await HubCommunicator.GetActivityTemplates();
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