using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using terminalFacebook.Controllers;
using terminalFacebook.Models;

namespace terminalFacebook.Infrastructure
{
    public class FacebookVerificationModelBinder : IModelBinder
    {

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var queryParams = actionContext.Request.GetQueryNameValuePairs().ToList();
            bindingContext.Model = new VerificationMessage
            {
                Challenge = int.Parse(queryParams.First(k => k.Key == "hub.challenge").Value),
                Mode = queryParams.First(k => k.Key == "hub.mode").Value,
                VerifyToken = queryParams.First(k => k.Key == "hub.verify_token").Value
            };
            return true;
        }
    }
}