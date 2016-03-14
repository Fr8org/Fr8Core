using System.Collections.Generic;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Services
{
    public interface IDocuSignManager
    {
        DropDownList CreateDocuSignTemplatePicker(bool addOnChangeEvent, string name = "Selected_DocuSign_Template", string label = "Select DocuSign Template");
        DocuSignTemplateDTO DownloadDocuSignTemplate(DocuSignAuthTokenDTO authToken, string templateId);
        Crate PackCrate_DocuSignTemplateNames(DocuSignAuthTokenDTO authToken);
        void FillDocuSignTemplateSource(Crate configurationCrate, string controlName,  DocuSignAuthTokenDTO authToken);
        void FillFolderSource(Crate configurationCrate, string controlName, DocuSignAuthTokenDTO authToken);
        void FillStatusSource(Crate configurationCrate, string controlName);
        StandardPayloadDataCM CreateActivityPayload(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO, string curEnvelopeId, string templateId);
        string GetEnvelopeIdFromPayload(PayloadDTO curPayloadDTO);
        int UpdateUserDefinedFields(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO, IUpdatableCrateStorage updater, string templateId, string envelopeId = null);

        /// <summary>
        /// Extracts fields from a DocuSign envelope.
        /// </summary>
        /// <param name="docuSignTemplateId">DocuSign TemplateId.</param>
        /// <param name="docuSignAuthToken">DocuSign authentication token.</param>
        /// <param name="curActivityDO">ActionDO object representing the current action. The crate with extracted 
        /// fields will be added to this Action replacing any older instances of that crate.</param>
        IEnumerable<FieldDTO> ExtractTemplateFieldsAndAddToCrate(string templateId, DocuSignAuthTokenDTO docuSignAuthToken, ActivityDO curActivityDO);

        Crate CrateCrateFromFields(string docuSignTemplateId, DocuSignAuthTokenDTO docuSignAuthToken, string crateLabel);
        int CountEnvelopes(DocuSignAuthTokenDTO authToken, DocusignQuery query);
        List<FolderItem> SearchDocusign(DocuSignAuthTokenDTO authToken, DocusignQuery query);
    }
}