using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pluginSalesforce.Infrastructure
{
    public interface ILead
    {
        bool CreateLead(ActionDTO actionDTO);
    }
}