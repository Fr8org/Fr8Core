using Data.Entities;
using PluginBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;
using Newtonsoft.Json;
using Core.Interfaces;
using StructureMap;
using System.Web.Http;
using System.Web.Http.Results;
using PluginBase;

namespace pluginDocuSign.Actions
{
    public class Extract_From_DocuSign_v1 : BasePluginAction
    {
        ICrate _crate = ObjectFactory.GetInstance<ICrate>();
        IAction _action = ObjectFactory.GetInstance<IAction>();

        public object Configure(ActionDO curActionDO)
        {
            //TODO: The coniguration feature for Docu Sign is not yet defined. The configuration evaluation needs to be implemented.
            return ProcessConfigurationRequest(curActionDO, actionDo => ConfigurationRequestType.Initial); // will be changed to complete the config feature for docu sign
        }

        public void Activate(ActionDO curActionDO)
        {
            return; // Will be changed when implementation is plumbed in.
        }

        public void Execute(ActionDO curActionDO)
        {
            //Get envlopeId
            string envelopeId = GetEnvelopeId(curActionDO);
            if (envelopeId == null)
            {
                throw new PluginCodedException(PluginErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");
            }


        }

        private string GetEnvelopeId(ActionDO curActionDO)
        {
            var crate = GetCrate(curActionDO, c => c.ManifestId == STANDARD_PAYLOAD_MANIFEST_ID, GetCrateDirection.Upstream);
            if (crate == null) return null; //TODO: log it
            var fields = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FieldDTO>>(crate.Contents);
            if (fields == null || fields.Count == 0)
            {
                return null; // TODO: log it
            }
            var envelopeIdField = fields.SingleOrDefault(f => f.Key == "EnvelopeId");
            if (envelopeIdField == null || string.IsNullOrEmpty(envelopeIdField.Value))
            {
                return null; // TODO: log it
            }
            return envelopeIdField.Value;

        }
    }
}