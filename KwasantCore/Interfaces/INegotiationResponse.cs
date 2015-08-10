using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace KwasantCore.Interfaces
{
    public interface INegotiationResponse
    {
        void ProcessEmailedResponse(IUnitOfWork uow, EmailDO emailDO, NegotiationDO negotiationDO, string userId);
    }
}
