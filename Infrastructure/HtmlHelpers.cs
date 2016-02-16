using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HubWeb.Infrastructure
{
    public static class HtmlHelpers
    {
        public static bool IsDebug(this HtmlHelper htmlHelper)
        {
            #if DEV || RELEASE
                    return false;
            #else
                  return true;
            #endif
        }
    }
}