
﻿using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
﻿using Data.Interfaces.ManifestSchemas;
﻿using Newtonsoft.Json.Linq;

namespace Core.Interfaces
{
    public interface ICrate 
    {
        CrateDTO Create(string label, string contents, string manifestType = "", int manifestId = 0);
        T GetContents<T>(CrateDTO crate);
        IEnumerable<JObject> GetElementByKey<TKey>(IEnumerable<CrateDTO> searchCrates, TKey key, string keyFieldName);
        CrateDTO CreateDesignTimeFieldsCrate(string label, List<FieldDTO> fields);
        CrateDTO CreateStandardConfigurationControlsCrate(string label, List<FieldDefinitionDTO> controls);
    }
}
