using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class AsanaTaskListCM : Manifest
    {
        public IEnumerable<AsanaTaskCM> Tasks;

        public AsanaTaskListCM() : base(MT.AsanaTaskList)
        {
        }  
    }
}
