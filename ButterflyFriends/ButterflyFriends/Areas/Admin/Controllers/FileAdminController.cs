using System.Web.Mvc;
using ButterflyFriends.Models;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    public class FileAdminController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        // GET: Admin/FileAdmin
        /// <summary>
        ///     returns thumbnails for search
        /// </summary>
        /// <param name="id">id of thumbnails</param>
        /// <returns>image if found, null otherwise</returns>
        [Authorize(Roles = "Eier,Ansatt,Admin")]
        public ActionResult Index(int id)
        {
            var fileToRetrieve = _context.ThumbNails.Find(id);
            if (fileToRetrieve == null)
                return null;
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }

        /// <summary>
        ///     returns a picture as long as you have employee or higer priviliges
        /// </summary>
        /// <param name="id">id of picture</param>
        /// <returns>the picture</returns>
        [Authorize(Roles = "Eier,Ansatt,Admin")]
        public ActionResult Picture(int id)
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve == null)
                return null;
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }
    }
}