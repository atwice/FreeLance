using System.Web;
using System.Web.Optimization;

namespace FreeLance
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

			bundles.Add(new ScriptBundle("~/bundles/jquery-signalR").Include(
						"~/Scripts/jquery.signalR-2.1.2.js"));

			bundles.Add(new ScriptBundle("~/bundles/jquery-comments/js").Include(
					  "~/Scripts/jquery-comments.js"));

			bundles.Add(new StyleBundle("~/bundles/jquery-comments/css").Include(
						"~/Content/jquery-comments.css"
						//"~/Content/font-awesome.min.css"
						));

			bundles.Add(new ScriptBundle("~/bundles/chat/js").Include(
						"~/Scripts/chat.js"));

			bundles.Add(new StyleBundle("~/bundles/chat/css").Include(
						"~/Content/chat.css"));
		}
	}
}
