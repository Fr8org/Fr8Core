using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IActionRegistration
    {
        IEnumerable<ActionTemplateDO> GetAll();
        ActionTemplateDO GetByKey(int curActionRegistrationDOId);
       // string AssemblePluginRegistrationName(ActionRegistrationDO curActionRegistrationDO);
    }
}
