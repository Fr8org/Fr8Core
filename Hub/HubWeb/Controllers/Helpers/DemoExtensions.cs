using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Json;

//using DayPilot.Web.Ui;


namespace HubWeb.Controllers.Helpers
{
    public static class DemoExtensions
    {
        public static IHtmlString GetDownloadLink(this HtmlHelper helper)
        {
            return new HtmlString(String.Format("<a href='{0}'>{1}</a>", GetDownloadUrl(helper), GetDownloadName(helper)));
        }

        public static string GetBuild(this HtmlHelper helper)
        {
            return Assembly.GetAssembly(typeof(global::DayPilot.Web.Mvc.DayPilotCalendar)).GetName().Version.ToString();
        }

        public static string GetDownloadName(this HtmlHelper helper)
        {
            Version v = Assembly.GetAssembly(typeof(DayPilotCalendar)).GetName().Version;
            return String.Format("DayPilotProMvcTrial-{0}.{1}.{2}.zip", v.Major, v.Minor, v.Build);
        }

        public static string GetDownloadUrl(this HtmlHelper helper)
        {
            bool isSandbox = helper.ViewContext.HttpContext.Request.Path.ToLower().Contains("sandbox");
            bool isDemo = helper.ViewContext.HttpContext.Request.Path.ToLower().Contains("BA");

            if (isSandbox)
            {
                return Resolve(helper, String.Format("~/{0}", GetDownloadName(helper)));
            }
            if (isDemo)
            {
                return String.Format("/files/{0}", GetDownloadName(helper));
            }
            return GetDownloadName(helper);
        }

        private static string Resolve(HtmlHelper helper, string url)
        {
            UrlHelper urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            return urlHelper.Content(url);
        }

        public static IHtmlString Menu(this HtmlHelper helper)
        {
            StringBuilder sb = new StringBuilder();
            HttpRequestBase request = helper.ViewContext.HttpContext.Request;
            string path = request.Path;

            // don't print menu on the front page)
            string fixedPath = request.Path.EndsWith("/") ? request.Path : request.Path + "/"; // fixes IIS vs. development server difference
            if (fixedPath.ToLower() == request.ApplicationPath.ToLower() + "/")
            {
                return new HtmlString(String.Empty);
            }

            string relative = path.Substring(request.ApplicationPath.Length);
            string dir = relative.Substring(1, relative.IndexOf('/', 1) - 1);
            string config = request.MapPath("~/Views/" + dir + "/Navigation.json");
            string action = relative.Substring(1 + dir.Length + 1);
            if (String.IsNullOrEmpty(action))
            {
                action = "/";
            }

            JsonData data = SimpleJsonDeserializer.Deserialize(File.ReadAllText(config));

            for (int i = 0; i < data.Count; i++)
            {
                JsonData tab = data[i];

                String url = (string)tab["url"];
                String title = (string)tab["title"];

                if (String.IsNullOrEmpty(url))
                {
                    sb.Append("<div class='header'>");
                    sb.Append(title);
                    sb.Append("</div>");
                }
                else
                {
                    sb.Append("<div><a href='");
                    if (url == "/")
                    {
                        sb.Append("./");
                    }
                    else
                    {
                        sb.Append(url);
                    }
                    sb.Append("'");

                    if (action == url)
                    {
                        sb.Append(" class='selected'");
                    }

                    sb.Append("><span>");
                    sb.Append(title);
                    //sb.Append(" a:" + action);
                    sb.Append("</span></a></div>");
                }
            }
            return new HtmlString(sb.ToString());
        }

        public static IHtmlString Tabs(this HtmlHelper helper)
        {
            StringBuilder sb = new StringBuilder();
            HttpRequestBase request = helper.ViewContext.HttpContext.Request;

            string dir = "/";
            string fixedPath = request.Path.EndsWith("/") ? request.Path : request.Path + "/"; // fixes IIS vs. development server difference
            if (fixedPath.ToLower() != request.ApplicationPath.ToLower() + "/")
            {
                string relative = request.Path.Substring(request.ApplicationPath.Length);
                dir = relative.Substring(1, relative.IndexOf('/', 1) - 1);
            }

            string config = request.MapPath("~/Demo.config");

            sb.Append("<div>");
            JsonData data = SimpleJsonDeserializer.Deserialize(File.ReadAllText(config));

            string description = String.Empty;
            for (int i = 0; i < data.Count; i++)
            {
                JsonData tab = data[i];

                String url = (string)tab["url"];
                String title = (string)tab["title"];

                sb.Append("<a class='");
                if (dir == url)
                {
                    description = (string)tab["description"];
                    sb.Append("tab selected");
                }
                else
                {
                    sb.Append("tab");
                }

                string path = (url == "/") ? url : "/" + url + "/";

                sb.Append("' href='");
                sb.Append(request.ApplicationPath + path);
                sb.Append("'><span style='width: 100px; text-align:center;'>");
                sb.Append(title);
                sb.Append("</span></a>");
            }

            sb.Append("</div>");

            sb.Append("<div class='header'><div class='bg-help'>");
            sb.Append(description);
            sb.Append("</div></div>");


            return new HtmlString(sb.ToString());
        }
    }
}
