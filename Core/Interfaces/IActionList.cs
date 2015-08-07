using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IActionList
    {
        IEnumerable<ActionListDO> GetAll();
        ActionListDO Get(int id);
        bool AddAction(ActionDO curActionDO, string position);
        void Process(ActionListDO curActionListDO);
    }
}
