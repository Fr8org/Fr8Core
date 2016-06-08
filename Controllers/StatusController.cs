using System;
using System.Diagnostics;
using System.Web.Mvc;



// DocuSign API Walkthrough 03 in C# - Get Recipient Status 
//
// To run this sample: 
// 	1) Create a new .NET project.
//	2) Add 4 assembly references to the project:  System, System.Net, System.XML, and System.XML.Linq
//	3) Update the email, password, integrator key, and envelopeId in the code
//	4) Compile and Run
//
// NOTE 1: The DocuSign REST API accepts both JSON and XML formatted http requests.  These C# API walkthroughs
// 	   demonstrate the use of XML format, whereas the other walkthroughs show examples in JSON format.
using System.IO; 
using System.Net;
using System.Xml;
using System.Xml.Linq;
using Fr8.Infrastructure.Utilities.Configuration;

namespace DocusignTutorial.Controllers 
{
	public class  StatusController : Controller
	{
		public  void Login ()
		{


            string username = CloudConfigurationManager.GetSetting("username") ?? "Not Found"; 			 
            string password = CloudConfigurationManager.GetSetting("password") ?? "Not Found"; 			 
            string integratorKey = CloudConfigurationManager.GetSetting("IntegratorKey") ?? "Not Found";			 
			
            //string envelopeId = "***";			// valid envelopeId of an envelope in your account
			//---------------------------------------------------------------------------------------------------
			
			// additional variable declarations
            string baseURL = CloudConfigurationManager.GetSetting("BaseUrl") ?? "Not Found"; ;			// - we will retrieve this through the Login API call
 
			try {
				//============================================================================
				//  STEP 1 - Login API Call (used to retrieve your baseUrl)
				//============================================================================
				
				// Endpoint for Login api call (in demo environment):
				string url = "https://demo.docusign.net/restapi/v2/login_information";
 
				// set request url, method, and headers.  No body needed for login api call
				HttpWebRequest request = initializeRequest( url, "GET", null, username, password, integratorKey);
				
				// read the http response
				string response = getResponseBody(request);
				
				// parse baseUrl from response body
				baseURL = parseDataFromResponse(response, "baseUrl");

			    Session["baseURL"] = baseURL;

				//--- display results
				Debug.Print("\nAPI Call Result: \n\n" + prettyPrintXml(response));
				
				
			}
			catch (WebException e) {
				using (WebResponse response = e.Response) {
					HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Debug.Print("Error code: {0}", httpResponse.StatusCode);
					using (Stream data = response.GetResponseStream())
					{
						string text = new StreamReader(data).ReadToEnd();
                        Debug.Print(prettyPrintXml(text));
					}
				}
			}
		} // end main()


        
		
		//***********************************************************************************************
		// --- HELPER FUNCTIONS ---
		//***********************************************************************************************
		public static HttpWebRequest initializeRequest(string url, string method, string body, string email, string password, string intKey)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (url);
			request.Method = method;
			addRequestHeaders( request, email, password, intKey );
			if( body != null )
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
			request.Headers.Add ("X-DocuSign-Authentication", authenticateStr);
			request.Accept = "application/xml";
			request.ContentType = "application/xml";
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static void addRequestBody(HttpWebRequest request, string requestBody)
		{
			// create byte array out of request body and add to the request object
			byte[] body = System.Text.Encoding.UTF8.GetBytes (requestBody);
			Stream dataStream = request.GetRequestStream ();
			dataStream.Write (body, 0, requestBody.Length);
			dataStream.Close ();
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static string getResponseBody(HttpWebRequest request)
		{
			// read the response stream into a local string
			HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse ();
			StreamReader sr = new StreamReader(webResponse.GetResponseStream());
			string responseText = sr.ReadToEnd();
			return responseText;
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static string parseDataFromResponse(string response, string searchToken)
		{
			// look for "searchToken" in the response body and parse its value
			using (XmlReader reader = XmlReader.Create(new StringReader(response))) {
				while (reader.Read()) {
					if((reader.NodeType == XmlNodeType.Element) && (reader.Name == searchToken))
						return reader.ReadString();
				}
			}
			return null;
		}
		/////////////////////////////////////////////////////////////////////////////////////////////////////////
		public static string prettyPrintXml(string xml)
		{
			// print nicely formatted xml
			try {
				XDocument doc = XDocument.Parse(xml);
				return doc.ToString();
			}
			catch (Exception) {
				return xml;
			}
		}
	} // end class
} // end namespace