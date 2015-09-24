using AutoMapper;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.Infrastructure.AutoMapper
{
	class RecipientsToRecipientsDTOConverter : ITypeConverter<Recipients, RecipientsDTO>
	{
		public RecipientsDTO Convert(ResolutionContext context)
		{
			var recipients = (Recipients)context.SourceValue;
			RecipientsDTO recipientsDTO = new RecipientsDTO();
			var allSigners = new List<Signer>(
				recipients.agents.Concat(recipients.carbonCopies)
				.Concat(recipients.certifiedDeliveries)
				.Concat(recipients.editors)
				.Concat(recipients.inPersonSigners)
				.Concat(recipients.intermediaries)
				.Concat(recipients.signers));
			foreach(var signer in allSigners)
			{
				RecipientDTO recipientDTO = Mapper.Map<RecipientDTO>(signer);
				recipientsDTO.Recipients.Add(recipientDTO);
			}
			return recipientsDTO;
		}
	}
}
