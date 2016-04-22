using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface IHistoryItemDO : IBaseDO
    {
        String PrimaryCategory { get; set; }
        String SecondaryCategory { get; set; }
        String Activity { get; set; }
        String Data { get; set; }
        String Status { get; set; }
        string ObjectId { get; set; }       
        string Fr8UserId { get; set; }
    }
}
