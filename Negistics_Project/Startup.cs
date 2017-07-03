using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Negistics_Project.Startup))]
namespace Negistics_Project
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
