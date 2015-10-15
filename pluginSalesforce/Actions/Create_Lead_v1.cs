using Data.Interfaces.DataTransferObjects;
using StructureMap;
using pluginSalesforce.Infrastructure;
using PluginUtilities.BaseClasses;
using System.Threading.Tasks;
using Data.Interfaces.ManifestSchemas;
using Salesforce.Common;
namespace pluginSalesforce.Actions
{
    public class Create_Lead_v1 : BasePluginAction
    {
        //ILead _salesforce = ObjectFactory.GetInstance<ILead>();

        public ActionDTO CreateLead(ActionDTO curActionDTO)
        {
            //bool result=_salesforce.CreateLead(curActionDTO);
            return curActionDTO;
        }

        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (IsEmptyAuthToken(curActionDTO))
            {
                AppendDockyardAuthenticationCrate(
                    curActionDTO,
                    AuthenticationMode.ExternalMode);

                return curActionDTO;
            }

            RemoveAuthenticationCrate(curActionDTO);

            return await ProcessConfigurationRequest(curActionDTO, x => ConfigurationEvaluator(x));
        }
    }
}