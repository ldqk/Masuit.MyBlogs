using System.Web.Optimization;

namespace Masuit.MyBlogs.WebApp
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            //javascript脚本
            bundles.Add(new ScriptBundle("~/js").Include(
                "~/Scripts/bootstrap-suggest.min.js",
                "~/Scripts/jquery.query.js",
                "~/Scripts/jquery.paging.js",
                "~/Scripts/ripplet.js",
                "~/Assets/tippy/tippy.js",
                "~/Scripts/global/scripts.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles").Include(
                 "~/Assets/newsbox/jquery.bootstrap.newsbox.js",
                 "~/Assets/tab/stopExecutionOnTimeout.js",
                 "~/Assets/tab/init.js",
                 //"~/Assets/modal/custombox.js",
                 "~/Assets/tagcloud/js/tagcloud.js",
                 "~/Assets/scrolltop/js/scrolltop.js",
                "~/Assets/nav/js/main.js"
                ));

            bundles.Add(new ScriptBundle("~/ng-views/filemgr").Include(
                 "~/ng-views/filemanager/js/app.js",
                 "~/ng-views/filemanager/js/directives/directives.js",
                 "~/ng-views/filemanager/js/filters/filters.js",
                 "~/ng-views/filemanager/js/providers/config.js",
                 "~/ng-views/filemanager/js/entities/chmod.js",
                 "~/ng-views/filemanager/js/entities/item.js",
                 "~/ng-views/filemanager/js/services/apihandler.js",
                 "~/ng-views/filemanager/js/services/apimiddleware.js",
                 "~/ng-views/filemanager/js/services/filenavigator.js",
                 "~/ng-views/filemanager/js/providers/translations.js",
                 "~/ng-views/filemanager/js/controllers/main.js",
                 "~/ng-views/filemanager/js/controllers/selector-controller.js"
                ));

            //css样式
            bundles.Add(new StyleBundle("~/css").Include(
                "~/Content/jquery.paging.css",
                "~/Content/common/reset.css",
                "~/Content/common/loading.css",
                "~/Content/common/style.css",
                "~/Content/common/articlestyle.css",
                "~/Content/common/leaderboard.css",
                "~/Content/microtip.min.css"
                ));
            bundles.Add(new StyleBundle("~/styles").Include(
                "~/Assets/breadcrumb/style.css",
                "~/Assets/nav/css/style.css",
                //"~/Assets/modal/custombox.css",
                //"~/Assets/modal/demo.css",
                "~/Assets/tab/styles.css",
                "~/Assets/tagcloud/css/tagcloud.css",
                "~/Assets/tippy/tippy.css"
                ));
        }
    }
}