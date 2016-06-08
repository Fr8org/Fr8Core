using System;
using System.Net;
using System.Web.Mvc;
using Fr8.Infrastructure.Utilities;

namespace HubWeb.Filters
{
    public class RequestParamsEncryptedFilter : ActionFilterAttribute, IActionFilter
    {
        public const string PARAMETER_NAME = "enc";

        public bool AllowPlainParams { get; set; }

        void OnPlainParams(ActionExecutingContext filterContext)
        {
            if (AllowPlainParams)
            {
                base.OnActionExecuting(filterContext);
            }
            else
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.QueryString.Count != 1)
            {
                OnPlainParams(filterContext);
                return;
            }

            // two options:
            // 1) ?enc=<encrypted_params>
            string encryptedParams = filterContext.HttpContext.Request.QueryString[PARAMETER_NAME];
            // 2) ?<encrypted_params>
            if (encryptedParams == null && filterContext.HttpContext.Request.QueryString.GetKey(0) == null)
            {
                encryptedParams = filterContext.HttpContext.Request.QueryString[0];
            }
            if (string.IsNullOrEmpty(encryptedParams))
            {
                OnPlainParams(filterContext);
                return;
            }
            try
            {
                var decryptedParams = Encryption.DecryptParams(encryptedParams);
                foreach (var actionParameter in decryptedParams)
                {
                    filterContext.ActionParameters[actionParameter.Key] = actionParameter.Value;
                }
                base.OnActionExecuting(filterContext);
/*
                var url = filterContext.HttpContext.Request.RawUrl;
                var urlBuilder = new StringBuilder(url);
                var indexOfParams = url.IndexOf('?') + 1;
                urlBuilder.Remove(indexOfParams, url.Length - indexOfParams);
                urlBuilder.Append(decryptedParamsStr);
                filterContext.Result = new RedirectResult(urlBuilder.ToString());
*/
            }
            catch (Exception)
            {
                OnPlainParams(filterContext);
            }
        }
    }
}