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
    public class ActionTemplate :IActionTemplate
    {
        public IEnumerable<ActionTemplateDO> GetAll()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActionTemplateRepository.GetAll();
            }
        }

        public ActionTemplateDO GetByKey(int curActionTemplateId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionTemplateDO = uow.ActionTemplateRepository.GetByKey(curActionTemplateId);
                if (curActionTemplateDO == null)
                    throw new ArgumentNullException("ActionTemplateId");

                return curActionTemplateDO;
            }

        }
    }
}
