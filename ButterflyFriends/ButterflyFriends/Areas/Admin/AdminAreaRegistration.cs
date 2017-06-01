using System.Web.Mvc;

namespace ButterflyFriends.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }
        
        /// <summary>
        /// Registers the admin paths so that it can be used in routevalues
        /// </summary>
        /// <param name="context"></param>
        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "ButterflyFriends.Areas.Admin.Controllers" }
            );
        }
    }
}