using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Core.Interfaces
{
    public interface ICrate 
    {
        CrateDTO Create(string label, string contents, string manifestType = "", int manifestId = 0);
        T GetContents<T>(CrateDTO crate);
        IEnumerable<JObject> GetElementByKey(IEnumerable<CrateDTO> searchCrates, string key);
    }
}
