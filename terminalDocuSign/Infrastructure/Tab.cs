using System.Collections.Generic;

using DocuSign.Integrations.Client;

using Data.Interfaces.DataTransferObjects;

namespace terminalDocuSign.Infrastructure
{
    public interface ITab
    {
        List<EnvelopeDataDTO> ExtractEnvelopeData(DocuSign.Integrations.Client.Envelope envelope, Signer curSigner);
    }

    public class Tab : DocuSign.Integrations.Client.Tab, ITab
    {
        public List<EnvelopeDataDTO> ExtractEnvelopeData(DocuSign.Integrations.Client.Envelope envelope, Signer curSigner)
        {
            List<EnvelopeDataDTO> curEnvelopeDataSet = new List<EnvelopeDataDTO>();

            Tabs curTabsSet = curSigner.tabs;
            if (curTabsSet != null)
            {
                foreach (TextTab curTextTab in curTabsSet.textTabs)
                {
                    EnvelopeDataDTO curEnvelopeData = new EnvelopeDataDTO
                                                   {
                                                       RecipientId = curSigner.recipientId,
                                                       EnvelopeId = envelope.EnvelopeId,
                                                       DocumentId = curTextTab.documentId,
                                                       Name = curTextTab.tabLabel,
                                                       TabId = curTextTab.tabId,
                                                       Value = curTextTab.value
                                                   };

                    curEnvelopeDataSet.Add(curEnvelopeData);
                }

                //TODO continue to do, all -> curTabsSet. tabs to envelope data ? Like below;
                //foreach (Tab curCheckBoxTab in curTabsSet.checkboxTabs)
                //{
                //    EnvelopeData curEnvelopeData = new EnvelopeData
                //                                   {
                //                                       RecipientId = curSigner.recipientId,
                //                                       EnvelopeId = envelope.EnvelopeId,
                //                                       DocumentName = curDocumentName,
                //                                       Name = curCheckBoxTab.name,
                //                                       TabId = curCheckBoxTab.tabId,
                //                                       Value = curCheckBoxTab.value
                //                                   };
                //TODO make for companyTabs, dateSignedTabs, emailTabs etc.

                //    curEnvelopeDataSet.Add(curEnvelopeData);
                //}
            }

            return curEnvelopeDataSet;
        }
    }
}
