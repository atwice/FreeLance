using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FreeLance.Startup))]
namespace FreeLance
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
			app.MapSignalR();
		}
    }
}
