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
        [Authorize(Roles = "Eier, Admin, Ansatt, Fadder")]
        public ActionResult Index(int id)
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve == null)
            {
                return null;
            }
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }
        [Authorize(Roles = "Eier, Admin, Ansatt, Fadder")]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult ProfilePicture(int id)
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve == null)
            {
                return null;
            }
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }
        [Authorize(Roles = "Eier, Admin, Ansatt, Fadder")]
        public ActionResult ArticleImg(int id)
        {
            var fileToRetrieve = _context.ThumbNails.Find(id);
            if (fileToRetrieve == null)
            {
                return null;
            }
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }

        public ActionResult ArticleImage(int id)    //you can reach this controller as anonumous, but you can only get article images
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve == null)
            {
                if(fileToRetrieve.FileType != DbTables.FileType.ArticleImage) { 
                return null;
                }
            }
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }

        public ActionResult Carousel(int id)    //you can reach this controller as anonumous, but you can only get carousel images
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve != null)
            {
                if (fileToRetrieve.FileType == DbTables.FileType.CarouselImage || fileToRetrieve.FileType == DbTables.FileType.CarouselVideo) { 
                return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
                }
            }
            return null;
        }

        public ActionResult Background(int id)    //you can reach this controller as anonumous, but you can only get background images
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve?.FileType == DbTables.FileType.BackgroundImage)
            {
                return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
            }
            return null;
        }

        public ActionResult Terms(int id)    //you can reach this controller as anonumous, but you can only get the website terms
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve?.FileType == DbTables.FileType.PDF)
            {
                return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
            }
            return null;
        }
    }
}