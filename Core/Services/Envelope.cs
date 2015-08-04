using System.Collections.Generic;
using System.Linq;

using Data.Interfaces;

using DocuSign.Integrations.Client;

using Utilities;

namespace Core.Services
{
    public class Envelope : DocuSign.Integrations.Client.Envelope, IEnvelope
    {
        private string _baseUrl;

        public Envelope()
        {

        }

        public List<EnvelopeData> GetEnvelopeData(DocuSign.Integrations.Client.Envelope envelope)
        {
            //Each EnvelopeData row is essentially a specific DocuSign "Tab".

            List<EnvelopeData> curEnvelopeDataSet = new List<EnvelopeData>();

            Signer[] curSignersSet = GetSignersFromRecipients(envelope);
            if (curSignersSet != null)
            {
                foreach (Signer curSigner in curSignersSet)
                {
                    FillEnvelopeForEveryTab(envelope, curSigner, curEnvelopeDataSet);
                }
            }

            return curEnvelopeDataSet;
        }

        private static Signer[] GetSignersFromRecipients(DocuSign.Integrations.Client.Envelope envelope)
        {
            return envelope.Recipients != null
                       ? envelope.Recipients.signers
                       : null;
        }

        private static void FillEnvelopeForEveryTab(DocuSign.Integrations.Client.Envelope envelope,
            Signer curSigner,
            List<EnvelopeData> curEnvelopeDataSet)
        {
            string curDocumentName = GetCurDocumentName(envelope);

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
        }

        private static string GetCurDocumentName(DocuSign.Integrations.Client.Envelope envelope)
        {
            string curDocumentName = string.Empty;

            //Note orkan -asking>: how to retreive document name properly? It may be more than one document in an envelope too...
            EnvelopeDocument firstOrDefault = envelope.GetDocuments().FirstOrDefault();
            if (firstOrDefault != null)
            {
                curDocumentName = firstOrDefault.name;
            }

            return curDocumentName;
        }
    }
}
