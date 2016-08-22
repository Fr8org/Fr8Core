using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hub.Interfaces
{
    public interface ITemplateGenerator
    {
        Uri BaseUrl { get; }

        string OutputFolder { get; }
        //TODO: make generated classes implement some specific base class or interface
        Task Generate(dynamic template, string fileName, IDictionary<string, object> parameters = null);
    }
}
