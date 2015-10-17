using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;

namespace PluginUtilities.Infrastructure
{
    public class ValidationDataTuple : Tuple<string, string, GetCrateDirection, string>
    {
        public ValidationDataTuple(string fieldName, string crateLabel, GetCrateDirection direction, string manifestType)
            : base(fieldName, crateLabel, direction, manifestType)
        {

        }

        public ValidationDataTuple(string fieldName) : base(fieldName, null, GetCrateDirection.None, null)
        {

        }
    }
}
