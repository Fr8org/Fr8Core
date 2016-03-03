using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Data
{
    public static class ExternalLogger
    {
        public static void Write(string format, params object [] p)
        {
            LogDb(string.Format(format, p));
        }

        class AsyncState
        {
            public HttpWebRequest Request;
            public string Message;

            public AsyncState(HttpWebRequest request, string message)
            {
                Request = request;
                Message = message;
            }
        }

        private static void LogDb(string message)
        {
            using (var connection = new SqlConnection("Data Source=tcp:s79ifqsqga.database.windows.net,1433;Initial Catalog=fr8Db2IntTest_20160303.34;User ID=alexeddodb@s79ifqsqga;Password=Zeus4593"))
            {
                connection.Open();

                using (var command = new SqlCommand("insert into [dbo].[TestLogs] (Message) values (@msg)"))
                {
                    command.Connection = connection;
                    command.Parameters.AddWithValue("@msg", message);
                    command.ExecuteNonQuery();
                }
            }
        }


        public static void Log(string message)
        {
            // Create a new HttpWebRequest object.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://46.39.254.29:8087");

            request.ContentType = "application/x-www-form-urlencoded";

            // Set the Method property to 'POST' to post data to the URI.
            request.Method = "POST";
            request.Proxy = new WebProxy();

            // start the asynchronous operation
            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), new AsyncState(request, message));
        }

        private static void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = ((AsyncState)asynchronousResult.AsyncState).Request;

            // End the operation
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

        // Convert the string into a byte array. 
            byte[] byteArray = Encoding.UTF8.GetBytes(((AsyncState)asynchronousResult.AsyncState).Message);

            // Write to the request stream.
            postStream.Write(byteArray, 0, ((AsyncState)asynchronousResult.AsyncState).Message.Length);
            postStream.Close();

            // Start the asynchronous operation to get the response
            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private static void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // End the operation
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse);
            string responseString = streamRead.ReadToEnd();
            
            // Close the stream object
            streamResponse.Close();
            streamRead.Close();

            // Release the HttpWebResponse
            response.Close();
        }
    }
}
