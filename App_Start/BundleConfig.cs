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

            bundles.Add(new StyleBundle("~/bundles/css/backendcss")
                .Include("~/Content/css/backendcss/default.css", new CssRewriteUrlTransform())
                );

            bundles.Add(new StyleBundle("~/bundles/css/fontawesome").Include(
                "~/Content/css/additionalcss/font-awesome/font-awesome.css", new CssRewriteUrlTransform()
                ));

            bundles.Add(new StyleBundle("~/bundles/css/select2").Include(
               "~/bower_components/select2/select2.css", new CssRewriteUrlTransform()
               ));

            bundles.Add(new StyleBundle("~/Content/css/font-awesome")
                .Include("~/bower_components/font-awesome-min/css/font-awesome.min.css", new CssRewriteUrlTransform())
            );

            bundles.Add(new StyleBundle("~/Content/css/main")
                .Include("~/bower_components/bootstrap/dist/css/bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/awesome-bootstrap-checkbox/awesome-bootstrap-checkbox.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/bootstrap-switch/dist/css/bootstrap3/bootstrap-switch.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/datatables/media/css/jquery.dataTables.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/datatables/media/css/dataTables.bootstrap.min.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/angular-datatables/dist/plugins/bootstrap/datatables.bootstrap.min.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/textAngular/dist/textAngular.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/ngToast/dist/ngToast.min.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/ng-table/dist/ng-table.min.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/angular-ui-select/dist/select.min.css", new CssRewriteUrlTransform())
                //DOC: To use 'rounded corners' style just load 'components-rounded.css' stylesheet instead of 'components.css' in the below style tag
                .Include("~/Content/templates/metronic/assets/global/css/components.css", new CssRewriteUrlTransform())
                .Include("~/Content/templates/metronic/assets/global/css/plugins.css", new CssRewriteUrlTransform())
                .Include("~/Content/templates/metronic/assets/admin/layout3/css/layout.css", new CssRewriteUrlTransform())
                .Include("~/Content/templates/metronic/assets/admin/layout3/css/themes/default.css", new CssRewriteUrlTransform())
                .Include("~/Content/templates/metronic/assets/admin/layout3/css/custom.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/dockyard.css", new CssRewriteUrlTransform())
            );

            bundles.Add(new ScriptBundle("~/bundles/jsunittests")
                .IncludeDirectory("~/Scripts/tests/utils/", "*.js", true)
                .IncludeDirectory("~/Scripts/tests/unit/", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/jsintegrationtests")
                .IncludeDirectory("~/Scripts/tests/utils/", "*.js", true)
                .IncludeDirectory("~/Scripts/tests/integration/", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/js/fr8Main")
                .Include("~/Scripts/app/_compiled.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/metronic")
                .Include("~/Content/templates/metronic/assets/global/scripts/metronic.js")
                .Include("~/Content/templates/metronic/assets/admin/layout3/scripts/layout.js")
                .Include("~/Content/templates/metronic/assets/admin/layout3/scripts/demo.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/js/jquery-bootstrap")
                .Include("~/bower_components/jquery/dist/jquery.js")
                .Include("~/bower_components/jquery-migrate/jquery-migrate.js")
                .Include("~/bower_components/bootstrap/dist/js/bootstrap.js")
                .Include("~/bower_components/bootstrap-hover-dropdown/bootstrap-hover-dropdown.js")
                .Include("~/bower_components/spin.js/spin.js")
                .Include("~/bower_components/bootstrap-switch/dist/js/bootstrap-switch.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/js/bower")
                .Include("~/bower_components/angular/angular.js")
                .Include("~/bower_components/angular-resource/angular-resource.js")
                .Include("~/bower_components/angular-animate/angular-animate.js")
                .Include("~/bower_components/angular-sanitize/angular-sanitize.js")
                .Include("~/bower_components/angular-ui-router/release/angular-ui-router.js")
#if DEBUG
                .Include("~/bower_components/angular-mocks/angular-mocks.js")
#endif
                .Include("~/bower_components/ocLazyLoad/dist/ocLazyLoad.js")
                .Include("~/bower_components/angular-bootstrap/ui-bootstrap-tpls.js")
                .Include("~/bower_components/underscore/underscore.js")
                .Include("~/bower_components/datatables/media/js/jquery.dataTables.min.js")
                .Include("~/bower_components/angular-datatables/dist/angular-datatables.js")
                .Include("~/bower_components/ng-table/dist/ng-table.min.js")
                .Include("~/bower_components/ng-file-upload/ng-file-upload-all.min.js")
                .Include("~/bower_components/pusher/dist/pusher.js")
                .Include("~/bower_components/pusher-angular/lib/pusher-angular.js")
                .Include("~/Scripts/lib/jquery.blockui.min.js")
                .Include("~/bower_components/ngToast/dist/ngToast.min.js")
                .Include("~/bower_components/mb-scrollbar/mb-scrollbar.min.js")
                .Include("~/bower_components/rangy/rangy-core.min.js")
                .Include("~/bower_components/rangy/rangy-selectionsaverestore.min.js")
                .Include("~/bower_components/textAngular/dist/textAngular-sanitize.min.js")
                .Include("~/bower_components/textAngular/dist/textAngular.min.js")
                .Include("~/bower_components/angular-bootstrap-switch/dist/angular-bootstrap-switch.js")
                .Include("~/bower_components/angular-ui-select/dist/select.min.js")
                .Include("~/bower_components/angular-applicationinsights/dist/angular-applicationinsights.min.js")
                .Include("~/bower_components/dndLists/angular-drag-and-drop-lists.min.js")
            );

#if RELEASE || DEV
            BundleTable.EnableOptimizations = true;
#endif

            //bundles.Add(new StyleBundle("~/bundles/css/temp").Include(
            //   "~/Content/css/temp/temp.css", new CssRewriteUrlTransform()
            //   ));         
        }
    }
}
