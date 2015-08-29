using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class ActionRegistration :IActionRegistration
    {
        public IEnumerable<ActionTemplateDO> GetAll()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActionRegistrationRepository.GetAll();
            }
        }

        public ActionTemplateDO GetByKey(int curActionRegistrationId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionRegistrationDO = uow.ActionRegistrationRepository.GetByKey(curActionRegistrationId);
                if (curActionRegistrationDO == null)
                    throw new ArgumentNullException("ActionRegistrationId");

                return curActionRegistrationDO;
            }

        }
    }
}
