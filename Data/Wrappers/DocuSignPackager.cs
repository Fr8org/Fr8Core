using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DocuSign.Integrations.Client;
using Microsoft.WindowsAzure;

namespace Data.Wrappers
{
    public class DocuSignPackager
    {

        public string Email;  //these are used to populate the Login object in the DocuSign Library
        public string ApiPassword;

        public DocuSignPackager()
        {
            ConfigureDocuSignIntegration();
        }

        public DocuSignAccount Login(string resource = "")
        {
            var username = Email ?? "Not Found";
            var password = ApiPassword ?? "Not Found";

            // set request url, method, and headers.  No body needed for login api call
            //HttpWebRequest request = initializeRequest(requestURL, "GET", null, username, password, integratorKey);

            // read the http response
            // string response = getResponseBody(request);
            //--- display results
            //Debug.Print("\nAPI Call Result: \n\n" + prettyPrintXml(response));

            // credentials for sending account
            Account account = new Account();
            account.Email = username;
            account.Password = password;

            // make the Login API call
            bool result = account.Login();

            if (!result)
            {
                throw new InvalidOperationException("Cannot log in to DocuSign. Please check the authentication information on web.config.");
            }
            return DocuSignAccount.Create(account);

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

        public void ConfigureDocuSignIntegration()
        {
            RestSettings.Instance.DistributorCode = CloudConfigurationManager.GetSetting("DocuSignDistributorCode");
            RestSettings.Instance.DistributorPassword = CloudConfigurationManager.GetSetting("DocuSignDistributorPassword");
            RestSettings.Instance.IntegratorKey = CloudConfigurationManager.GetSetting("DocuSignIntegratorKey");
            RestSettings.Instance.DocuSignAddress = CloudConfigurationManager.GetSetting("environment");
            RestSettings.Instance.WebServiceUrl = RestSettings.Instance.DocuSignAddress + "/restapi/v2";

            Email = CloudConfigurationManager.GetSetting("DocuSignLoginEmail");
            ApiPassword = CloudConfigurationManager.GetSetting("DocuSignLoginPassword");
        }

        //public string GetEnvelope(Dictionary<string,string> queryParams)
        //{
        //    string paramString = "?from_date=" + queryParams["from_date"];
        //    return Login("envelopes" + paramString);
        //}

        //***********************************************************************************************
        // --- HELPER FUNCTIONS ---
        //***********************************************************************************************
        public static HttpWebRequest InitializeRequest(string url, string method, string body, string email, string password, string intKey)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            AddRequestHeaders(request, email, password, intKey);
            if (body != null)
                AddRequestBody(request, body);
            return request;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void AddRequestHeaders(HttpWebRequest request, string email, string password, string intKey)
        {
            // authentication header can be in JSON or XML format.  XML used for this walkthrough:
            var authenticateStr =
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
        public static void AddRequestBody(HttpWebRequest request, string requestBody)
        {
            // create byte array out of request body and add to the request object
            byte[] body = Encoding.UTF8.GetBytes(requestBody);
            var dataStream = request.GetRequestStream();

            dataStream.Write(body, 0, requestBody.Length);
            dataStream.Close();
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string GetResponseBody(HttpWebRequest request)
        {
            // read the response stream into a local string
            var webResponse = (HttpWebResponse)request.GetResponse();
            var sr = new StreamReader(webResponse.GetResponseStream());
            var responseText = sr.ReadToEnd();

            return responseText;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string ParseDataFromResponse(string response, string searchToken)
        {
            // look for "searchToken" in the response body and parse its value
            using (var reader = XmlReader.Create(new StringReader(response)))
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
        public static string PrettyPrintXml(string xml)
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

        public DocuSignAccount LoginAsDockyardService()
        {
            var email = Email ?? "Not Found";
            var password = ApiPassword ?? "Not Found";

            var curAccount = new Account
            {
                Email = email,
                ApiPassword = password
            };
            var curDocuSignAccount = DocuSignAccount.Create(curAccount);

            if (curDocuSignAccount.Login())
                return curDocuSignAccount;

            throw new InvalidOperationException(
                "Cannot log in to DocuSign. Please check the authentication information on web.config.");
        }
    }
}