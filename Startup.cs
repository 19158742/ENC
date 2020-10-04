using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ENC.Startup))]
namespace ENC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
