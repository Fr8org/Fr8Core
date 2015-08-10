using System;
using KwasantCore.Managers.APIManagers.Transmitters.Restful;

namespace KwasantCore.ExternalServices.REST
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
