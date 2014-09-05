using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ContosoUniversityCF.Startup))]
namespace ContosoUniversityCF
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
