using System;
using Core.Managers.APIManagers.Transmitters.Restful;

namespace Core.ExternalServices.REST
{
    public class RestfulResponseWrapper : IRestfulResponse
    {
        private RestfulResponse _response;
        public RestfulResponseWrapper(RestfulResponse response)
        {
            _response = response;
        }

        public string Content
        {
            get { return _response.Content; }
        }
    }
}
