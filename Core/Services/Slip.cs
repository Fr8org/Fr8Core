using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using StructureMap;


namespace Core.Services
{
    public class Metaflow
    {

        public int Create()
        {
           
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                SlipDO curFlow = new SlipDO();
                curFlow.Name = "Flow_" + DateTime.Now;
                uow.SlipRepository.Add(curFlow);
                uow.SaveChanges();
                return curFlow.Id;
            }
        }


    }
}
