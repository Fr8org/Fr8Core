using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using terminalInstagram.Models;

namespace terminalInstagram.Infrastructure
{
    public class InstagramVerificationModelBinder : IModelBinder
    {

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var queryParams = actionContext.Request.GetQueryNameValuePairs().ToList();
            bindingContext.Model = new VerificationMessage
            {
                Challenge = queryParams.First(k => k.Key == "hub.challenge").Value,
                Mode = queryParams.First(k => k.Key == "hub.mode").Value,
                VerifyToken = queryParams.First(k => k.Key == "hub.verify_token").Value
            };
            return true;
        }
    }
}