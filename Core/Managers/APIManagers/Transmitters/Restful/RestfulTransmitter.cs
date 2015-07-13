using System;
using System.IO;
using System.Net;

namespace Core.Managers.APIManagers.Transmitters.Restful
{
    /// <summary>
    /// This is a common Class for call any Restful Call.Contains the methods that call the API , Get the response and 
    /// convert that response into RestfulResponse using StreamReader.
    /// Also this class is used for specific Authentication.
    /// </summary>
    public class RestfulTransmitter
    {
        public RestfulResponse Transmit(HttpWebRequest curWebRequest, RestfulCall curCall)
        {
            return GetResponse(curWebRequest);
        }

        /// <summary>
        /// Gets http web response
        /// </summary>
        private RestfulResponse GetResponse(HttpWebRequest curWebRequest)
        {
            RestfulResponse curResponse = null;
            try
            {
                HttpWebResponse curWebResponse = (HttpWebResponse)curWebRequest.GetResponse();
                curResponse = ConvertToRestResponse(curWebResponse);
            }
            catch (Exception ex)
            {
                throw new Exception("Error: Reading API Response" + ex.Message.ToString());
            }
            return curResponse;
        }

        /// <summary>
        /// Convert httpwebresponse to custom rest response
        /// </summary>
        private RestfulResponse ConvertToRestResponse(HttpWebResponse curWebResponse)
        {
            StreamReader sr = new StreamReader(curWebResponse.GetResponseStream());
            RestfulResponse curResponse = new RestfulResponse();
            curResponse.Content = sr.ReadToEnd();
            curResponse.StatusCode = curWebResponse.StatusCode;
            curResponse.StatusDescription = curWebResponse.StatusDescription;
            foreach (string headerName in curWebResponse.Headers.AllKeys)
            {
                string headerValue = curWebResponse.Headers[headerName];
                curResponse.Headers.Add(headerName, headerValue);
            }
            return curResponse;
        }
    }
}
