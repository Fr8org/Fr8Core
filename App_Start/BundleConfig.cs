using System.Web.Optimization;

namespace HubWeb.App_Start
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //Scripts
            bundles.Add(new ScriptBundle("~/bundles/js/modernizr").Include(
                "~/bower_components/modernizr/modernizr.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/jquery").Include(
                "~/bower_components/jquery/dist/jquery.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap").Include(
                "~/bower_components/bootstrap/dist/js/bootstrap.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap31").Include(
                "~/bower_components/bootstrap/js/bootstrap.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/colorbox").Include(
                "~/bower_components/jquery-colorbox/jquery.colorbox.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/waypoints").Include(
                "~/bower_components/waypoints/waypoints.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/placeholder").Include(
                "~/bower_components/jquery-placeholder/jquery.placeholder.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/main").Include(
                "~/Scripts/_legacy/main.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/aboutpage").Include(
                "~/Scripts/_legacy/about-page.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/customjs").Include(
                "~/Scripts/_legacy/custom.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/select2").Include(
                "~/bower_components/select2/select2.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap-datetimepicker").Include(
                "~/bower_components/eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/jqueryvalidate").Include(
                "~/bower_components/jquery-validation/dist/jquery.validate.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/moment").Include(
                "~/bower_components/moment/min/moment.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstraptransition").Include(
                "~/bower_components/bootstrap/js/transition.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/collapse").Include(
                "~/bower_components/bootstrap/js/collapse.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/js/cookies").Include(
                "~/bower_components/jquery.cookie-min/jquery.cookie.js"
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

            bundles.Add(new StyleBundle("~/bundles/css/select2").Include(
               "~/bower_components/select2/select2.css", new CssRewriteUrlTransform()
               ));

            bundles.Add(new ScriptBundle("~/bundles/jsunittests")
                .IncludeDirectory("~/Scripts/tests/utils/", "*.js", true)
                .IncludeDirectory("~/Scripts/tests/unit/", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/jsintegrationtests")
                .IncludeDirectory("~/Scripts/tests/utils/", "*.js", true)
                .IncludeDirectory("~/Scripts/tests/integration/", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/fr8Main")
                .Include("~/Scripts/app/_compiled.js"));

#if RELEASE || DEV
            BundleTable.EnableOptimizations = true;
#endif

            //bundles.Add(new StyleBundle("~/bundles/css/temp").Include(
            //   "~/Content/css/temp/temp.css", new CssRewriteUrlTransform()
            //   ));         
        }
    }
}
