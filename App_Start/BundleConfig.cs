using System.Web.Optimization;

namespace Web.App_Start
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //Scripts
            bundles.Add(new ScriptBundle("~/bundles/js/modernizr").Include(
                "~/Content/js/modernizr.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/jquery").Include(
                "~/Content/js/jquery.js"
                ));

			bundles.Add(new ScriptBundle("~/bundles/js/bootstrap").Include(
				"~/Content/js/bootstrap.js",
				"~/Content/js/bootstrap-responsive.js"
				));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap31").Include(
				"~/Content/js/bootstrap.3.1.1.min.js"
                ));

			bundles.Add(new ScriptBundle("~/bundles/js/jquery").Include(
				"~/Content/js/jquery.js"
				));

            bundles.Add(new ScriptBundle("~/bundles/js/colorbox").Include(
                "~/Content/js/jquery.colorbox.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/waypoints").Include(
                "~/Content/js/waypoints.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/placeholder").Include(
                "~/Content/js/placeholder.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/main").Include(
                "~/Content/js/main.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/aboutpage").Include(
                "~/Content/js/about-page.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/common").Include(
                "~/Content/js/KwasantCommon.js"
                ));

			bundles.Add(new ScriptBundle("~/bundles/js/customjs").Include(
				"~/Content/js/custom.js"
				));

            bundles.Add(new ScriptBundle("~/bundles/js/kwasantpopup").Include(
                "~/Content/js/Kwasant/Popup.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/kwasantcalendar").Include(
                "~/Content/js/KwasantCalendar.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/daypilot").Include(
                "~/Content/js/Daypilot/daypilot-common.src.js",
                "~/Content/js/Daypilot/daypilot-bubble.src.js",
                "~/Content/js/Daypilot/daypilot-calendar.src.js",
                "~/Content/js/Daypilot/daypilot-datepicker.src.js",
                "~/Content/js/Daypilot/daypilot-menu.src.js",
                "~/Content/js/Daypilot/daypilot-modal.src.js",
                "~/Content/js/Daypilot/daypilot-month.src.js",
                "~/Content/js/Daypilot/daypilot-navigator.src.js",
                "~/Content/js/Daypilot/daypilot-scheduler.src.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/select2").Include(
                "~/Content/js/select2.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap-datetimepicker").Include(
                "~/Content/js/bootstrap-datetimepicker.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/jqueryvalidate").Include(
                "~/Content/js/jquery.validate.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/moment").Include(
                "~/Content/js/moment.js"
                ));

			bundles.Add(new ScriptBundle("~/bundles/js/bootstraptransition").Include(
				"~/Content/js/bootstrap-transition.js"
				));

			bundles.Add(new ScriptBundle("~/bundles/js/collapse").Include(
				"~/Content/js/collapse.js"
				));

            bundles.Add(new ScriptBundle("~/bundles/js/cookies").Include(
                "~/Content/js/jquery.cookie-1.4.1.min.js"
                ));

            //Styles
            bundles.Add(new StyleBundle("~/bundles/css/bootstrap23").Include(
				"~/Content/css/additionalcss/bootstrap23/css/bootstrap2.3.css", new CssRewriteUrlTransform()
                ));

            bundles.Add(new StyleBundle("~/bundles/css/bootstrap30").Include(
				"~/Content/css/additionalcss/bootstrap30/css/bootstrap3.0.css", new CssRewriteUrlTransform()
                ));

			bundles.Add(new StyleBundle("~/bundles/css/bootstrap-responsive").Include(
				"~/Content/css/additionalcss/bootstrap30/css/bootstrap-responsive.css", new CssRewriteUrlTransform()
				));

			bundles.Add(new StyleBundle("~/bundles/css/bootstrap-datetimepicker").Include(
				"~/Content/css/additionalcss/bootstrap30/css/bootstrap-datetimepicker.css", new CssRewriteUrlTransform()
			    ));

            bundles.Add(new StyleBundle("~/bundles/css/colorbox").Include(
				"~/Content/css/additionalcss/colorbox/colorbox.css", new CssRewriteUrlTransform()
                ));

            bundles.Add(new StyleBundle("~/bundles/css/frontpage").Include(
				"~/Content/css/frontcss/main.css", new CssRewriteUrlTransform()
                ));

			bundles.Add(new StyleBundle("~/bundles/css/backendcss").Include(
				"~/Content/css/backendcss/default.css", new CssRewriteUrlTransform()
				));

            bundles.Add(new StyleBundle("~/bundles/css/fontawesome").Include(
				"~/Content/css/additionalcss/font-awesome/font-awesome.css", new CssRewriteUrlTransform()
                ));

            bundles.Add(new StyleBundle("~/bundles/css/daypilot").Include(
				"~/Content/css/Daypilot/*_green.css", new CssRewriteUrlTransform()
                ));

            bundles.Add(new StyleBundle("~/bundles/css/select2").Include(
			   "~/Content/css/additionalcss/select2/select2.css", new CssRewriteUrlTransform()
               ));

            //bundles.Add(new StyleBundle("~/bundles/css/temp").Include(
            //   "~/Content/css/temp/temp.css", new CssRewriteUrlTransform()
            //   ));         
        }
    }
}
