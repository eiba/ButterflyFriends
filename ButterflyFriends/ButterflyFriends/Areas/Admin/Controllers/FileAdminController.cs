using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ButterflyFriends.Models;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    public class FileAdminController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        // GET: Admin/FileAdmin
        public ActionResult Index(int id)
        {
            var fileToRetrieve = _context.ThumbNails.Find(id);
            if (fileToRetrieve == null)
            {
                return null;
            }
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }
    }
}