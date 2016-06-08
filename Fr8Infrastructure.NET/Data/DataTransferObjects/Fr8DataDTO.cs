using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class Fr8DataDTO
    {
        public ActivityDTO ActivityDTO { get; set; }
        public Guid? ContainerId { get; set; }
        
        /// <summary>
        /// This property is used for integration tests
        /// </summary>
        public string ExplicitData { get; set; }
    }
}
