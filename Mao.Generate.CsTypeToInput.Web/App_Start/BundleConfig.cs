using System.Web;
using System.Web.Optimization;

namespace Mao.Generate.CsTypeToInput.Web
{
    public class BundleConfig
    {
        // 如需統合的詳細資訊，請瀏覽 https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/bundles/css").Include(
                "~/lib/fontawesome-free-5.15.3-web/css/all.min.css",                                                // fontawesome
                "~/lib/bootstrap-4.6.0-dist/css/bootstrap.min.css",                                                 // bootstrap
                "~/lib/bootstrap-datepicker-1.9.0-dist/css/bootstrap-datepicker.min.css",                           // bootstrap-datepicker
                "~/lib/bootstrap-table-1.18.3-dist/bootstrap-table.min.css",                                        // bootstrap-table
                "~/lib/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/lib/jquery-3.6.0.min.js",                                                                        // jquery
                "~/lib/fontawesome-free-5.15.3-web/js/all.min.js",                                                  // fontawesome
                "~/lib/popper.js-1.16.1/umd/popper.min.js",                                                         // * bootstrap 需要的參考
                "~/lib/bootstrap-4.6.0-dist/js/bootstrap.min.js",                                                   // bootstrap
                "~/lib/bootstrap-datepicker-1.9.0-dist/js/bootstrap-datepicker.min.js",                             // bootstrap-datepicker
                "~/lib/bootstrap-table-1.18.3-dist/bootstrap-table.min.js",                                         // bootstrap-table
                "~/lib/site.js"));
        }
    }
}
