using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Web.Mvc;
using DocuSign.Integrations.Client;
using Microsoft.WindowsAzure;

namespace Core.Managers.APIManagers.Packagers.Docusign
{
    public class DocuSignPackager
    {

        public string Email;  //these are used to populate the Login object in the DocuSign Library
        public string ApiPassword;

        public DocuSignPackager()
        {
            ConfigureDocuSignIntegration();
        }

        public string Login()
        {
            return MakeRequest("");
        } // 
        public void ConfigureDocuSignIntegration()
        {
            RestSettings.Instance.DistributorCode = CloudConfigurationManager.GetSetting("DocuSignDistributorCode");
            RestSettings.Instance.DistributorPassword = CloudConfigurationManager.GetSetting("DocuSignDistributorPassword");
            RestSettings.Instance.IntegratorKey = CloudConfigurationManager.GetSetting("DocuSignIntegratorKey");

            Email = CloudConfigurationManager.GetSetting("DocuSignLoginEmail");
            ApiPassword = CloudConfigurationManager.GetSetting("DocuSignLoginPassword");
        }

        public string MakeRequest(string resource)
        {
 
            string username = Email ?? "Not Found";
            string password = ApiPassword ?? "Not Found";
            string integratorKey = RestSettings.Instance.IntegratorKey ?? "Not Found";

            var appSettings = ConfigurationManager.AppSettings;
            string baseURL = appSettings["BaseUrL"];
            string requestURL = baseURL + resource;

            // set request url, method, and headers.  No body needed for login api call
            HttpWebRequest request = initializeRequest(requestURL, "GET", null, username, password, integratorKey);

            // read the http response
            string response = getResponseBody(request);
            //--- display results
            Debug.Print("\nAPI Call Result: \n\n" + prettyPrintXml(response));

            return response;

            //}
            //catch (WebException e)
            //{
            //    using (WebResponse response = e.Response)
            //    {
            //        HttpWebResponse httpResponse = (HttpWebResponse)response;
            //        Debug.Print("Error code: {0}", httpResponse.StatusCode);
            //        using (Stream data = response.GetResponseStream())
            //        {
            //            string text = new StreamReader(data).ReadToEnd();
            //            Debug.Print(prettyPrintXml(text));
            //        }
            //    }
            //}
        } // 

        public string GetEnvelope(Dictionary<string,string> queryParams )
        {
            string paramString = "?from_date=" + queryParams["from_date"];
            return MakeRequest("envelopes" + paramString);
        }




        //***********************************************************************************************
        // --- HELPER FUNCTIONS ---
        //***********************************************************************************************
        public static HttpWebRequest initializeRequest(string url, string method, string body, string email, string password, string intKey)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            addRequestHeaders(request, email, password, intKey);
            if (body != null)
                addRequestBody(request, body);
            return request;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void addRequestHeaders(HttpWebRequest request, string email, string password, string intKey)
        {
            // authentication header can be in JSON or XML format.  XML used for this walkthrough:
            string authenticateStr =
                "<DocuSignCredentials>" +
                    "<Username>" + email + "</Username>" +
                    "<Password>" + password + "</Password>" +
                    "<IntegratorKey>" + intKey + "</IntegratorKey>" +
                    "</DocuSignCredentials>";
            request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
            request.Accept = "application/xml";
            request.ContentType = "application/xml";
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void addRequestBody(HttpWebRequest request, string requestBody)
        {
            // create byte array out of request body and add to the request object
            byte[] body = System.Text.Encoding.UTF8.GetBytes(requestBody);
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(body, 0, requestBody.Length);
            dataStream.Close();
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string getResponseBody(HttpWebRequest request)
        {
            // read the response stream into a local string
            HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(webResponse.GetResponseStream());
            string responseText = sr.ReadToEnd();
            return responseText;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string parseDataFromResponse(string response, string searchToken)
        {
            // look for "searchToken" in the response body and parse its value
            using (XmlReader reader = XmlReader.Create(new StringReader(response)))
            {
                while (reader.Read())
                {
                    if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == searchToken))
                        return reader.ReadString();
                }
            }
            return null;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string prettyPrintXml(string xml)
        {
            // print nicely formatted xml
            try
            {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            }
            catch (Exception)
            {
                return xml;
            }
        }
    }
}
