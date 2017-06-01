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
        /// <summary>
        /// returns thumbnails for search
        /// </summary>
        /// <param name="id"></param>
        /// <returns>image if found, null otherwise</returns>
        [Authorize(Roles = "Eier,Ansatt,Admin")]
        public ActionResult Index(int id)
        {
            var fileToRetrieve = _context.ThumbNails.Find(id);
            if (fileToRetrieve == null)
            {
                return null;
            }
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }

        [Authorize(Roles = "Eier,Ansatt,Admin")]
        public ActionResult Picture(int id)
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve == null)
            {
                return null;
            }
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }
    }
}