using AutoMapper;
using System.Collections.Generic;

namespace terminalDocuSign.Infrastructure
{
    public interface ISigner
    {
        Signer[] GetFromRecipients(DocuSign.Integrations.Client.Envelope envelope);
    }

    public class Signer : DocuSign.Integrations.Client.Signer, ISigner
    {
        public Signer[] GetFromRecipients(DocuSign.Integrations.Client.Envelope envelope)
        {
            List<Signer> signers = new List<Signer>();

            if (envelope.Recipients != null)
            {
                foreach (var signer in envelope.Recipients.signers)
                {
                    signers.Add(Mapper.Map<Signer>(signer));
                }
                return signers.ToArray();
            }
            return null;
        }
    }
}
