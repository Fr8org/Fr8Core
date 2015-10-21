using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Data.Interfaces.DataTransferObjects
{
    public class FieldDTO
    {
        public string Key { get; set; }
        public string Value { get; set; }

        //TODO: Don't use this property, this temporary fix up for FilterUsingRunTimeData_v1#ParseCriteriaExpression. Refer details in (DO-1394)
        public int IntValue
        {
            get
            {
                return this.Value.ToInt();
            }
        }
    }
}
