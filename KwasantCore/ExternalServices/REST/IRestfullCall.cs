using System;
using Utilities;

namespace KwasantCore.ExternalServices.REST
{
    public interface IRestfullCall
    {
        void Initialize(String baseURL, String resource, Method method);
        void AddBody(String body, String contentType);
        IRestfulResponse Execute();
    }
}
