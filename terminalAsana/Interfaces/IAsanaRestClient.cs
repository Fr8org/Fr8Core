using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminalAsana.Interfaces
{
    public interface IAsanaRestClient
    {
        string SendAsync(Uri uri, string content, Dictionary<string,string> headers);
    }
}
