using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DocuSign.Integrations.Client;
using Newtonsoft.Json;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Interfaces;

namespace terminalDocuSign.Services
{
    public class DocuSignFolder : IDocuSignFolder
    {
        private readonly DocuSignPackager _packager;

        public DocuSignFolder()
        {
            _packager = new DocuSignPackager();
        }

        public List<DocusignFolderInfo> GetFolders(string login, string password)
        {
            var accout = _packager.Login(login, password);

            return MakeRequest<FolderListResponse>("/folders", accout).Folders;
        }


        public List<DocusignFolderInfo> GetSearchFolders(string login, string password)
        {
            return GetFolders(login, password).Where(x => x.Type != "report").ToList();
        }

        public List<FolderItem> Search(string login, string password, string searchText, string folderId, string status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(folderId)) throw new ArgumentNullException("folderId");
            
            StringBuilder queryBuilder = new StringBuilder("/folders/");
            
            queryBuilder.Append(folderId);

            queryBuilder.Append("?searchText=");
            if (String.IsNullOrWhiteSpace(searchText))
            {
                queryBuilder.Append(HttpUtility.UrlEncode(searchText));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                queryBuilder.Append("&status=");
                queryBuilder.Append(HttpUtility.UrlEncode(status));
            }

            if (fromDate != null)
            {
                queryBuilder.Append("&from_date=");
                queryBuilder.Append(HttpUtility.UrlEncode(fromDate.Value.ToString("dd-MM-yyyy | HH:mm")));
            }

            if (toDate != null)
            {
                queryBuilder.Append("&to_date=");
                queryBuilder.Append(HttpUtility.UrlEncode(toDate.Value.ToString("dd-MM-yyyy | HH:mm")));
            }

            queryBuilder.Append("&start_position=");
            
            var accout = _packager.Login(login, password);
            var urlTemplate = queryBuilder.ToString();
            int startPosition = 0;
            
            var items = new List<FolderItem>();

            while (true)
            {
                var url = urlTemplate + startPosition;
                var result = MakeRequest<FolderSearchResults>(url, accout);

                if (result.FolderItems != null)
                {
                    items.AddRange(result.FolderItems);
                }

                if (result.EndPosition < result.TotalSetSize)
                {
                    startPosition = result.EndPosition + 1;
                }
                else
                {
                    break;
                }
            }

            return items.ToList();
        }

        private static T MakeRequest<T>(string queryString, DocuSignAccount account, string payload = null)
        {
            RequestBuilder utils = new RequestBuilder();
            RequestInfo requestInfo = new RequestInfo();
            List<RequestBody> list = new List<RequestBody>();

            requestInfo.RequestContentType = "application/json";
            requestInfo.AcceptContentType = "application/json";
            requestInfo.HttpMethod = "GET";
            requestInfo.LoginEmail = account.Email;
            requestInfo.ApiPassword = account.ApiPassword;
            requestInfo.DistributorCode = RestSettings.Instance.DistributorCode;
            requestInfo.DistributorPassword = RestSettings.Instance.DistributorPassword;
            requestInfo.IntegratorKey = RestSettings.Instance.IntegratorKey;
            
            requestInfo.Uri = string.Format("{0}/{1}", account.BaseUrl, queryString);

            utils.Request = requestInfo;

            if (!string.IsNullOrEmpty(payload))
            {
                list.Add(new RequestBody
                {
                    Text = payload
                });
                requestInfo.RequestBody = list.ToArray();
                utils.Request = requestInfo;
            }
            
            ResponseInfo response = utils.MakeRESTRequest();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Error restError;

                try
                {
                    restError = Error.FromJson(response.ResponseText);
                }
                catch
                {
                    restError = new Error
                    {
                        message = response.ResponseText
                    };
                }
                restError.httpStatusCode = response.StatusCode;
                
                throw new DocusginRestException(restError);
            }

            return JsonConvert.DeserializeObject<T>(response.ResponseText);
        }
    }
}