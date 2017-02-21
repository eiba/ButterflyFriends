using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ButterflyFriends.Startup))]
namespace ButterflyFriends
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
