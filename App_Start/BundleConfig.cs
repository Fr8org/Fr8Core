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

            bundles.Add(new StyleBundle("~/bundles/css/frontpage")
                .Include("~/Content/css/homecss/bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/frontcss/main.css", new CssRewriteUrlTransform()));
            
            bundles.Add(new StyleBundle("~/bundles/css/frontpage-new")
                .Include("~/Content/css/homecss/bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/shared/main.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/frontcss/main_new.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/shared/navbar.css", new CssRewriteUrlTransform()));

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

            bundles.Add(new StyleBundle("~/Content/css/bower-no-cdn")
                .Include("~/bower_components/angular-datatables/dist/plugins/bootstrap/datatables.bootstrap.min.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/ngToast/dist/ngToast.min.css", new CssRewriteUrlTransform())
                //DOC: To use 'rounded corners' style just load 'components-rounded.css' stylesheet instead of 'components.css' in the below style tag
                .Include("~/Content/templates/metronic/assets/global/css/components.css", new CssRewriteUrlTransform())
                .Include("~/Content/templates/metronic/assets/global/css/plugins.css", new CssRewriteUrlTransform())
                .Include("~/Content/templates/metronic/assets/admin/layout3/css/layout.css", new CssRewriteUrlTransform())
                .Include("~/Content/templates/metronic/assets/admin/layout3/css/themes/default.css", new CssRewriteUrlTransform())
                .Include("~/Content/templates/metronic/assets/admin/layout3/css/custom.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/angular-ivh-treeview/dist/ivh-treeview.min.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/angular-ivh-treeview/dist/ivh-treeview-theme-basic.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/angular-resizable/angular-resizable.min.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/md-color-picker/dist/mdColorPicker.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/dockyard.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/dockyard/container-transition.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/dockyard/query-builder.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/dockyard/authentication-dialog.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/dockyard/activity-stream.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/dockyard/collapse.css", new CssRewriteUrlTransform())
                .Include("~/bower_components/angular-material-data-table/dist/md-data-table.min.css", new CssRewriteUrlTransform())
            );

            bundles.Add(new ScriptBundle("~/bundles/jsunittests")
                .IncludeDirectory("~/Scripts/tests/utils/", "*.js", true)
                .IncludeDirectory("~/Scripts/tests/unit/", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/jsintegrationtests")
                .IncludeDirectory("~/Scripts/tests/utils/", "*.js", true)
                .IncludeDirectory("~/Scripts/tests/integration/", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/js/fr8")
#if DEV || RELEASE
                .Include("~/Scripts/templateCache.js")
#else
                .Include("~/Scripts/dummyTemplates.js")
#endif
                .Include("~/Scripts/app/app.js")
                .Include("~/Scripts/app/_compiled.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/metronic")
                .Include("~/Content/templates/metronic/assets/global/scripts/metronic.js")
                .Include("~/Content/templates/metronic/assets/admin/layout3/scripts/layout.js")
                .Include("~/Content/templates/metronic/assets/admin/layout3/scripts/demo.js")
            );

            // Bundles for the new Home (video-loop)
            bundles.Add(new ScriptBundle("~/bundles/js/home")
                .Include("~/Scripts/homejs/jquery-2.1.0.min.js")
                .Include("~/Scripts/homejs/bootstrap.min.js")
                .Include("~/Scripts/homejs/plugins/scrollto/jquery.scrollTo-1.4.3.1-min.js")
                .Include("~/Scripts/homejs/plugins/scrollto/jquery.localscroll-1.2.7-min.js")
                .Include("~/Scripts/homejs/plugins/easing/jquery.easing.min.js")
                .Include("~/Scripts/homejs/plugins/parallax/jquery.parallax-1.1.3.js")
                //.Include("~/Scripts/homejs/plugins/twitter/twitter-fetcher.js")
                .Include("~/Scripts/homejs/plugins/jpreloader/jpreloader.min.js")
                .Include("~/Scripts/homejs/plugins/isotope/imagesloaded.pkgd.js")
                .Include("~/Scripts/homejs/plugins/isotope/isotope.pkgd.min.js")
                .Include("~/Scripts/homejs/plugins/wow/wow.js")
                .Include("~/Scripts/homejs/plugins/flexslider/jquery.flexslider-min.js")
                .Include("~/Scripts/homejs/plugins/magnific/jquery.magnific-popup.min.js")
                .Include("~/Scripts/homejs/plugins/parsley/parsley.min.js")
                .Include("~/Scripts/homejs/plugins/easypiechart/jquery.easypiechart.min.js")
                .Include("~/Scripts/homejs/plugins/waypoints/waypoints.min.js")
                .Include("~/Scripts/homejs/plugins/vide/jquery.vide.min.js")
                .Include("~/Scripts/homejs/loop.js")                
            );

            bundles.Add(new ScriptBundle("~/bundles/js/html5shiv")
                .Include("~/Scripts/homejs/html5shiv.js")
            );

            bundles.Add(new StyleBundle("~/bundles/css/home")
                .Include("~/Content/css/homecss/bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/homecss/font-awesome.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/shared/main.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/homecss/main.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/shared/navbar.css", new CssRewriteUrlTransform())
                .Include("~/Content/css/CDNRelatedStyles.css")
            );

            bundles.Add(new StyleBundle("~/bundles/css/homeie")
                .Include("~/Content/css/homecss/ie.css", new CssRewriteUrlTransform())
            );


            bundles.Add(new ScriptBundle("~/bundles/js/bower-no-cdn")
#if DEBUG
                .Include("~/bower_components/angular-mocks/angular-mocks.js")
#endif
                .Include("~/bower_components/ocLazyLoad/dist/ocLazyLoad.js") //not found on cdn
                .Include("~/bower_components/angular-datatables/dist/angular-datatables.js")//not found on cdn
                .Include("~/bower_components/ngToast/dist/ngToast.min.js")//not found on cdn
                .Include("~/bower_components/mb-scrollbar/mb-scrollbar.min.js")//not found on cdn
                .Include("~/bower_components/angular-bootstrap-switch/dist/angular-bootstrap-switch.js")//not found on cdn
                .Include("~/bower_components/angular-applicationinsights/dist/angular-applicationinsights.min.js")//not found on cdn
                .Include("~/bower_components/angular-ivh-treeview/dist/ivh-treeview.min.js")//not found on cdn
                .Include("~/bower_components/angular-resizable/angular-resizable.min.js")//not found on cdn
                .Include("~/bower_components/tinycolor/dist/tinycolor-min.js")//not found on cdn
                .Include("~/bower_components/angular-popover-toggle/popover-toggle.js")//not found on cdn
                .Include("~/bower_components/md-color-picker/dist/mdColorPicker.min.js")//not found on cdn
                .Include("~/bower_components/angular-material-data-table/dist/md-data-table.min.js")
                .Include("~/bower_components/jquery.kinetic/jquery.kinetic.min.js")
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
