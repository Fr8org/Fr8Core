using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Configuration
{
    public interface IApplicationSettings
    {
        string GetSetting(string name);
    }
}
