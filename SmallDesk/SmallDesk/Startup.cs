using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SmallDesk.Startup))]
namespace SmallDesk
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
