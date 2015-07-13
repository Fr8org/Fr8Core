using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Utilities;

namespace Core.Managers.APIManagers.Transmitters.Restful
{
    /// <summary>
    /// Container for data used to make requests
    /// </summary>
    public class RestfulCall
    {
        private string url;
        private RestfulTransmitter APITransmitter;
        /// <summary>
        /// URL to call and should include scheme and domain without trailing slash.
        /// </summary>
        public string BaseUrl
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
                if (url != null && url.EndsWith("/"))
                {
                    url = url.Substring(0, url.Length - 1);
                }
            }
        }

       

        /// <summary>
        /// Container of all HTTP header parameters to be passed with the request. 
        /// See AddHeader() for explanation of the types of parameters that can be passed
        /// </summary>
        public Dictionary<string, string> HeaderParams { get; private set; }

        /// <summary>
        /// Container of all HTTP query parameters to be passed with the request. 
        /// See AddQuery() for explanation of the types of parameters that can be passed
        /// </summary>
        public Dictionary<string, string> QueryParams { get; private set; }

        /// <summary>
        /// HTTP verb default is GET
        /// </summary>
        private Method method = Method.GET;

        /// <summary>
        /// Determines what HTTP method to use for this request. Supported methods: GET, POST, PUT
        /// Default is GET
        /// </summary>
        public Method Method
        {
            get { return method; }
            set { method = value; }
        }

        /// <summary>
        /// The Resource URL to make the request against.
        /// Should not include the scheme or domain. Do not include leading slash.
        /// Combined with RestAPIManager.BaseUrl to assemble final URL:
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Container for the request body.
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// Content type of the request body.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Default is true. Determine whether or not requests that result in 
        /// HTTP status codes of 3xx should follow returned redirect
        /// </summary>
        public bool AllowAutoRedirect { get; set; }

        /// <summary>
        /// Restricted header action which needs to exculde for amazon and oauth signature generation
        /// </summary>
        private IDictionary<string, Action<HttpWebRequest, string>> restrictedHeaderActions;

        /// <summary>
        /// Default constructor. Initialize rest Parameters and sets Auto Redirect to true.
        /// </summary>
        public RestfulCall()
        {
            HeaderParams = new Dictionary<string, string>();
            QueryParams = new Dictionary<string, string>();
            // Add default header parameters
            AddHeader("Accept", "application/json");
            AddHeader("Content-Type", "application/json");
            //Adds restricted headers which needs to exclude for signature generation
            restrictedHeaderActions = new Dictionary<string, Action<HttpWebRequest, string>>(StringComparer.OrdinalIgnoreCase);
            restrictedHeaderActions.Add("Accept", (r, v) => r.Accept = v);
            restrictedHeaderActions.Add("Content-Type", (r, v) => r.ContentType = v);
            //By default allow auto redirects
            AllowAutoRedirect = true;
            APITransmitter = new RestfulTransmitter();
        }

        /// <summary>
        /// Calls base constructor and sets Resource and Method properties
        /// </summary>
        public RestfulCall(string baseUrl, string resource, Method method)
            : this()
        {
            Resource = resource;
            Method = method;
            BaseUrl = baseUrl;
            APITransmitter = new RestfulTransmitter();
        }

        /// <summary>
        /// Adds Body parameter to this request instance
        /// </summary>
        public void AddBody(string bodyParameter, string contentType)
        {
            // passing the content type as the parameter name because there can only be
            // one parameter with ParameterType.RequestBody so name isn't used otherwise
            Body = bodyParameter;
            ContentType = contentType;
        }

        /// <summary>
        /// Adds Query parameters to this request instance
        /// </summary>
        public void AddQuery(string name, string value)
        {
            QueryParams.Add(name, value);
        }

        /// <summary>
        /// Adds Header parameters to this request instance
        /// </summary>
        public void AddHeader(string name, string value)
        {
            HeaderParams.Add(name, value);
        }

        /// <summary>
        /// Assembles URL to call based on parameters, method and resource
        /// </summary>
        private Uri BuildUrl()
        {
            string urlString = Resource;
            if (!urlString.IsNullOrEmpty() && urlString.StartsWith("/"))
                urlString = urlString.Substring(1);

            if (!BaseUrl.IsNullOrEmpty())
            {
                if (urlString.IsNullOrEmpty())
                    urlString = BaseUrl;
                else
                    urlString = string.Format("{0}/{1}", BaseUrl, urlString);
            }

            if (Method != Method.POST
                    && Method != Method.PUT)
            {
                // build and attach querystring if this is a get-style request
                if (QueryParams.Count > 0)
                    urlString = string.Format("{0}?{1}", urlString, BuildQueryString());
            }
            return urlString.AsUri();
        }

        /// <summary>
        /// Prepares httpwebrequest from restapirequest and writes to stream
        /// </summary>
        private void WriteBody(HttpWebRequest curWebRequest)
        {
            PrepareBody();
            curWebRequest.ContentType = ContentType;
            if (Body.IsNullOrEmpty())
            {
                curWebRequest.ContentLength = 0;
                return;
            }
            //prepare bytes for rest api request body for writing to stream
            byte[] requestBodyBytes = Encoding.UTF8.GetBytes(Body);
            curWebRequest.ContentLength = requestBodyBytes.Length;
            using (Stream requestStream = curWebRequest.GetRequestStream())
            {
                requestStream.Write(requestBodyBytes, 0, requestBodyBytes.Length);
            }
        }

        /// <summary>
        /// Prepares rest request for post body and set the content type accordingly
        /// </summary>
        private void PrepareBody()
        {
            //If rest request has post parameters then set content type to application/x-www-form-urlencoded"
            if (QueryParams.Count > 0)
            {
                string body = BuildQueryString();
                if (!body.IsNullOrEmpty())
                {
                    ContentType = "application/x-www-form-urlencoded";
                    Body = body;
                }
            }
        }

        /// <summary>
        /// Configures http web request using rest api request
        /// </summary>
        public HttpWebRequest configureCurCall()
        {
            HttpWebRequest curWebRequest = (HttpWebRequest)WebRequest.Create(BuildUrl());
            AppendHeaders(curWebRequest);
            curWebRequest.Method = Method.GetName();
            curWebRequest.AllowAutoRedirect = AllowAutoRedirect;
            curWebRequest.ContentType = ContentType;
            if (Method == Method.POST || Method == Method.PUT)
                WriteBody(curWebRequest);
            return curWebRequest;
        }

        /// <summary>
        /// Add headers from current call header parameters to current HTTP web request 
        /// Don't add restricted header parameters
        /// </summary>
        private void AppendHeaders(HttpWebRequest curWebRequest)
        {
            foreach (KeyValuePair<string, string> header in HeaderParams)
            {
                if (restrictedHeaderActions.ContainsKey(header.Key))
                    restrictedHeaderActions[header.Key].Invoke(curWebRequest, header.Value);
                else
                    curWebRequest.Headers.Add(header.Key, header.Value);
            }
        }

        /// <summary>
        /// Builds query string for get and post parameters
        /// </summary>
        private string BuildQueryString()
        {
            StringBuilder querystring = new StringBuilder();
            foreach (KeyValuePair<string, string> p in QueryParams)
            {
                if (querystring.Length > 1)
                    querystring.Append("&");
                querystring.AppendFormat("{0}={1}", p.Key, p.Value);
            }
            return querystring.ToString();
        }

        /// <summary>
        /// prepares HTTP verb request and executes the request
        /// </summary>
        public RestfulResponse Execute()
        {
            //Authenticate for Amazon/OAuth
            AuthenticateIfNeeded();

            HttpWebRequest curWebRequest = configureCurCall();
            return APITransmitter.Transmit(curWebRequest, this);
        }

        /// <summary>
        /// Authenticates with OAuth1 or Amazon as per instances set
        /// </summary>
        private void AuthenticateIfNeeded()
        {
           
        }
    }
}
