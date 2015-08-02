using System.Collections.Generic;
using System.Linq;

using DocuSign.Integrations.Client;

using Utilities;

namespace Core.Services
{
    public interface IEnvelope
    {
        List<EnvelopeData> GetEnvelopeData(DocuSign.Integrations.Client.Envelope docusignEnvelope);
    }

    public class Envelope : IEnvelope
    {
        private string _baseUrl;
        
        public Envelope()
        {

        }

        public List<EnvelopeData> GetEnvelopeData(DocuSign.Integrations.Client.Envelope docusignEnvelope)
        {
            //TODO orkan: when Dockyard retrieves this envelope data, it immediately normalizes it into a set of EnvelopeData objects. 
            //Each EnvelopeData row is essentially a specific DocuSign "Tab". By normalizing in this way, 
            //other parts of the system can make neat queries against this in-memory table. 
            //As a security policy, we do not intend to persist these DocumentData rows; we will generate them fresh from the retrieved DocuSign data each time we need them.

            //Each EnvelopeData row is essentially a specific DocuSign "Tab".

            List<EnvelopeData> envelopeDatas = new List<EnvelopeData>();

            Recipients recipients = docusignEnvelope.Recipients;

            if (recipients != null)
            {
                Signer[] signers = recipients.signers;

                //Note orkan -asking>: how to retreive document name properly? It may be more than one document in an envelope too...
                string documentName = string.Empty;
                EnvelopeDocument firstOrDefault = docusignEnvelope.GetDocuments().FirstOrDefault();
                if (firstOrDefault != null)
                {
                    documentName = firstOrDefault.name;
                }

                foreach (Signer signer in signers)
                {
                    Tabs tabs = signer.tabs;

                    if (tabs != null)
                    {
                        TextTab[] textTabs = tabs.textTabs;
                        Tab[] checkboxTabs = tabs.checkboxTabs;
                        Tab[] companyTabs = tabs.companyTabs;
                        DateSignedTab[] dateSignedTabs = tabs.dateSignedTabs;
                        Tab[] emailTabs = tabs.emailTabs;
                        Tab[] fullNameTabs = tabs.fullNameTabs;
                        Tab[] initialHereTabs = tabs.initialHereTabs;
                        RadioGroupTab[] radioGroupTabs = tabs.radioGroupTabs;

                        //Note orkan -asking> Do we want iterate each tabs with foreach and fill them like below ?
                        foreach (TextTab textTab in textTabs)
                        {
                            EnvelopeData envelopeData = new EnvelopeData
                            {
                                RecipientId = signer.recipientId,
                                EnvelopeId = docusignEnvelope.EnvelopeId,
                                DocumentName = documentName,
                                Name = textTab.name,
                                TabId = textTab.tabId,
                                Value = textTab.value
                            };

                            envelopeDatas.Add(envelopeData);
                        }
                    }
                }

            }

            return envelopeDatas;
        }
    }
}
