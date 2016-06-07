using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fr8.Infrastructure.Data.Constants;

namespace fr8.Infrastructure.Data.Manifests
{
    public class StandardFileListCM : Manifest
    {
        public List<StandardFileDescriptionCM> FileList { get; set; }

        public StandardFileListCM()
			  :base(MT.StandardFileList)
        {
        }
    }
}
