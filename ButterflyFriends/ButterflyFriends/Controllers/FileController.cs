using System.Web.Mvc;
using ButterflyFriends.Models;
using Microsoft.AspNet.Identity;

namespace ButterflyFriends.Controllers
{
    public class FileController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        // GET: File
        /// <summary>
        ///     Return protected images in database, but only if user has the right to see
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Eier, Admin, Ansatt, Fadder")] //check that user is logged in
        public ActionResult Index(int id)
        {
            var userID = User.Identity.GetUserId(); //if of current user
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve?.FileType != DbTables.FileType.Picture)
                //it's not a picture filetype, return null. If image not found also return null
                return null;
            foreach (var user in fileToRetrieve.User)
                if (user.Id == userID)
                    return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
            //check that user is actually tagged in picture, else retrun null
            foreach (var child in fileToRetrieve.Children)
                if (child.SponsorId == userID)
                    return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
            //check that user har children tagged in the picture
            return null;
        }

        /// <summary>
        ///     Return profile image
        /// </summary>
        /// <param name="id">id of image</param>
        /// <returns></returns>
        [Authorize(Roles = "Eier, Admin, Ansatt, Fadder")]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")] //disable caching for immediate display
        public ActionResult ProfilePicture(int id)
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve == null)
                return null;
            if (fileToRetrieve.FileType != DbTables.FileType.Profile)
                return null; //not a profile image
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }

        /// <summary>
        ///     return profile thumbnails
        /// </summary>
        /// <param name="id">Id of image</param>
        /// <returns>thumnail image</returns>
        [Authorize(Roles = "Eier, Admin, Ansatt, Fadder")] // check if user is logged in
        public ActionResult ArticleImg(int id)
        {
            var fileToRetrieve = _context.ThumbNails.Find(id);
            if (fileToRetrieve == null)
                return null;
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }

        /// <summary>
        ///     Return article images
        /// </summary>
        /// <param name="id">id of image</param>
        /// <returns>Article image</returns>
        public ActionResult ArticleImage(int id)
            //you can reach this controller as anonumous, but you can only get article images
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve == null)
                if (fileToRetrieve.FileType != DbTables.FileType.ArticleImage) return null;
            return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
        }

        /// <summary>
        ///     Return carousel images if exist
        /// </summary>
        /// <param name="id">id og image</param>
        /// <returns>Carousel image</returns>
        public ActionResult Carousel(int id)
            //you can reach this controller as anonymous, but you can only get carousel images
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve != null)
                if ((fileToRetrieve.FileType == DbTables.FileType.CarouselImage) ||
                    (fileToRetrieve.FileType == DbTables.FileType.CarouselVideo))
                    return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
            return null;
        }

        /// <summary>
        ///     Return backgroundimage if exist
        /// </summary>
        /// <param name="id">id of image</param>
        /// <returns>background image</returns>
        public ActionResult Background(int id)
            //you can reach this controller as anonymous, but you can only get background images
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve?.FileType == DbTables.FileType.BackgroundImage)
                return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
            return null;
        }

        /// <summary>
        ///     Return the terms of use if exist
        /// </summary>
        /// <param name="id">id of image</param>
        /// <returns>terms of use pdf</returns>
        public ActionResult Terms(int id)
            //you can reach this controller as anonymous, but you can only get the website terms
        {
            var fileToRetrieve = _context.Files.Find(id);
            if (fileToRetrieve?.FileType == DbTables.FileType.PDF)
                return File(fileToRetrieve.Content, fileToRetrieve.ContentType);
            return null;
        }
    }
}