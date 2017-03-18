using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ButterflyFriends.Models;

namespace ButterflyFriends.Controllers
{
    public class FileController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        // GET: File
        
        public ActionResult Index(int id)
        {
            var fileToRetrieve = _context.Files.Find(id);
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ProfilePicture(int id)
        {
            var fileToRetrieve = _context.Files.Find(id);
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }
    }
}