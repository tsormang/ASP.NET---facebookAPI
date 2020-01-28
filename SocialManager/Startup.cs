using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SocialManager.Startup))]
namespace SocialManager
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
