using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    public interface IExpectedResponse
    {
        void MarkAsStale(IUnitOfWork uow, ExpectedResponseDO expectedResponseDO);
    }
}
