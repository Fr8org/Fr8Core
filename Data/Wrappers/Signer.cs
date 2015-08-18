namespace Data.Wrappers
{
    public interface ISigner
    {
        Signer[] GetFromRecipients(DocuSign.Integrations.Client.Envelope envelope);
    }

    public class Signer : DocuSign.Integrations.Client.Signer, ISigner
    {
        public Signer[] GetFromRecipients(DocuSign.Integrations.Client.Envelope envelope)
        {
            if (envelope.Recipients != null)
            {
                return envelope.Recipients.signers as Signer[];
            }

            return null;
        }
    }
}
