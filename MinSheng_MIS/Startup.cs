using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MinSheng_MIS.Startup))]
namespace MinSheng_MIS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
