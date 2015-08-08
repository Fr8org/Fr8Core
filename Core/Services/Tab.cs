using System.Collections.Generic;

using DocuSign.Integrations.Client;

using Utilities;

namespace Core.Services
{
    public interface ITab
    {
        List<EnvelopeData> ExtractEnvelopeData(DocuSign.Integrations.Client.Envelope envelope, Signer curSigner);
    }

    public class Tab : DocuSign.Integrations.Client.Tab, ITab
    {
        public List<EnvelopeData> ExtractEnvelopeData(DocuSign.Integrations.Client.Envelope envelope, Signer curSigner)
        {
            List<EnvelopeData> curEnvelopeDataSet = new List<EnvelopeData>();

            Tabs curTabsSet = curSigner.tabs;
            if (curTabsSet != null)
            {
                foreach (TextTab curTextTab in curTabsSet.textTabs)
                {
                    EnvelopeData curEnvelopeData = new EnvelopeData
                                                   {
                                                       RecipientId = curSigner.recipientId,
                                                       EnvelopeId = envelope.EnvelopeId,
                                                       DocumentId = curTextTab.documentId,
                                                       Name = curTextTab.name,
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
