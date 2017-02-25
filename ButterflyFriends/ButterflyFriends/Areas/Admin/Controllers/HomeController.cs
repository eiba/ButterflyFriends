using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    [Authorize(Roles = "Owner, Admin")]
    public class HomeController : Controller
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}