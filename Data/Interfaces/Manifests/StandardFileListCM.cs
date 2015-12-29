using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.Manifests
{
    public class StandardFileListCM : Manifest
    {
        public List<StandardFileDescriptionCM> FileList { get; set; }

        public StandardFileListCM()
			  :base(Constants.MT.StandardFileList)
        {
        }
    }
}
