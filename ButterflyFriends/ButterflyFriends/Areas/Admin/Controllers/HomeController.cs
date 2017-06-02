using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ButterflyFriends.Models;
using Newtonsoft.Json;
using PagedList;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    [Authorize(Roles = "Eier,Ansatt,Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        public int pageSize = 4; //image per page
        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        ///     get initial images, filter by upload date then filid initially
        /// </summary>
        /// <returns>return view with list</returns>
        public ActionResult Images()
        {
            ViewBag.page = 1;
            var images = from s in _context.Files
                where
                s.FileType == DbTables.FileType.Picture
                orderby s.UploadDate.Value descending
                orderby s.FileId descending
                select s;

            return View(images.ToPagedList(1, pageSize));
        }

        /// <summary>
        ///     Get images based on filter
        /// </summary>
        /// <param name="page">current page</param>
        /// <returns>filter image result</returns>
        [HttpPost]
        public ActionResult GetImages(int? page)
        {
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var date = Request.Form["date"];
            var sponsor = Request.Form["user"];
            var child = Request.Form["child"];
            var filename = Request.Form["filename"];

            var pageNumber = page ?? 1;
            ViewBag.page = pageNumber;
            return FilterImagesResult(search, active, order, filter, date, sponsor, child, filename, pageNumber);
        }

        /// <summary>
        ///     Get images based on filter
        /// </summary>
        /// <returns>filter image result</returns>
        [HttpPost]
        public ActionResult FilterImages()
        {
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var date = Request.Form["date"];
            var sponsor = Request.Form["user"];
            var child = Request.Form["child"];
            var filename = Request.Form["filename"];

            var pageNumber = 1;
            ViewBag.page = 1;

            return FilterImagesResult(search, active, order, filter, date, sponsor, child, filename, pageNumber);
        }

        /// <summary>
        ///     Filter the images based on parameters
        /// </summary>
        /// <param name="search">search parameter</param>
        /// <param name="active">active entity or not</param>
        /// <param name="order">orderby</param>
        /// <param name="filter">filterby</param>
        /// <param name="date">image date</param>
        /// <param name="sponsor">sponsor tagged in image</param>
        /// <param name="child">child tagged in image</param>
        /// <param name="filename">name of file</param>
        /// <param name="pageNumber">current page</param>
        /// <returns>return partial view result containing image list</returns>
        public PartialViewResult FilterImagesResult(string search, string active, string order, string filter,
            string date, string sponsor, string child, string filename, int pageNumber)
        {
            DateTime? Date = new DateTime();
            if (!string.IsNullOrEmpty(date)) //date is not null
                Date = DateTime.Parse(date); //parse string date to actual datetime variable
            else
                Date = null;
            var images = from s in _context.Files
                where
                (s.FileType == DbTables.FileType.Picture) &&
                s.FileName.Contains(filename)
                select s;

            if (!string.IsNullOrEmpty(search))
                images = images.Where(s => s.Caption.Contains(search));
            if (Date.HasValue)
                images = images.Where(s => s.UploadDate.Value.Equals(Date.Value));
            if (active == "yes")
                images = images.Where(s => s.Published);
            else if (active == "no")
                images = images.Where(s => !s.Published);

            if (order == "descending")
                switch (filter)
                {
                    case "1":
                        images = images.OrderByDescending(s => s.Caption).ThenBy(s => s.FileId);
                        break;
                    case "2":
                        images = images.OrderByDescending(s => s.FileName);
                        break;
                    default:
                        images = images.OrderByDescending(s => s.UploadDate.Value).ThenByDescending(s => s.FileId);
                        break;
                }
            else
                switch (filter)
                {
                    case "1":
                        images = images.OrderBy(s => s.Caption).ThenBy(s => s.FileId);
                        break;
                    case "2":
                        images = images.OrderByDescending(s => s.FileName);
                        break;
                    default:
                        images = images.OrderBy(s => s.UploadDate.Value).ThenBy(s => s.FileId);
                        break;
                }

            if (!string.IsNullOrEmpty(sponsor) || (!string.IsNullOrEmpty(child) && images.Any()))
                //get images with spesific users and children
            {
                var imageList = new List<DbTables.File>();
                IList<int> ids = new List<int>();
                if (!string.IsNullOrEmpty(sponsor)) //Get images with spesific sponsor
                    foreach (var image in images.ToList())
                        if (image.User.Any() && !ids.Contains(image.FileId))
                            foreach (var user in image.User)
                                if ((user.Fname + " " + user.Lname).ToLower().Contains(sponsor))
                                {
                                    imageList.Add(image);
                                    ids.Add(image.FileId);
                                    break;
                                }
                if (!string.IsNullOrEmpty(child) && !string.IsNullOrEmpty(sponsor) && imageList.Any())
                    //get images with both the sent in sponsor and child in it
                {
                    var imageListChildren = new List<DbTables.File>();
                    IList<int> idsChildren = new List<int>();

                    foreach (var image in imageList)
                        if (image.Children.Any() && !idsChildren.Contains(image.FileId))
                            foreach (var Child in image.Children)
                                if ((Child.Fname + " " + Child.Lname).ToLower().Contains(child))
                                {
                                    imageListChildren.Add(image);
                                    idsChildren.Add(image.FileId);
                                    break;
                                }
                    images = imageListChildren.AsQueryable();
                }
                else if (string.IsNullOrEmpty(sponsor) && !string.IsNullOrEmpty(child)) //get images with child
                {
                    foreach (var image in images.ToList())
                        if (image.Children.Any() && !ids.Contains(image.FileId))
                            foreach (var Child in image.Children)
                                if ((Child.Fname + " " + Child.Lname).ToLower().Contains(child))
                                {
                                    imageList.Add(image);
                                    ids.Add(image.FileId);
                                    break;
                                }
                    images = imageList.AsQueryable();
                }
                else
                {
                    images = imageList.AsQueryable();
                }
            }
            if ((pageNumber > 1) && images.Any() && (images.Count() <= pageSize*pageNumber))
                //ensure that the page is not empty. Go to highest available page if empty page
            {
                pageNumber = (int) Math.Ceiling(images.Count()/(double) pageSize);
                ViewBag.page = pageNumber;
                return PartialView("_ImagesPartial", images.ToPagedList(pageNumber, pageSize));
            }
            if (!images.Any()) //no images left, got to page 1
            {
                ViewBag.page = 1;
                pageNumber = 1;
            }
            return PartialView("_ImagesPartial", images.ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        ///     upload image view
        /// </summary>
        /// <returns>upload image view</returns>
        public ActionResult Upload()
        {
            return View();
        }

        /// <summary>
        ///     Get current active users and children maching the search criteria and database rowaccsess optimization
        /// </summary>
        /// <returns>Json list of database entities</returns>
        [HttpPost]
        public ActionResult GetUsers()
        {
            var body = Request.InputStream;
            var encoding = Request.ContentEncoding;
            var reader = new StreamReader(body, encoding);
            var json = reader.ReadToEnd(); //get JSON from request


            var enteties = new List<object>();


            //get active users with name using an optimized query to only get useful rows
            var users =
                _context.Users.Where(s => (s.Fname + " " + s.Lname).Contains(json))
                    .OrderBy(p => p.Lname)
                    .Select(x => new UserObj
                    {
                        name = x.Fname + " " + x.Lname,
                        thumbnail = x.Thumbnail,
                        type = "user",
                        userId = x.Id,
                        email = x.Email
                    }).ToList();

            var children =
                _context.Children.Where(s => (s.Fname + " " + s.Lname).Contains(json))
                    .OrderBy(p => p.Lname)
                    .Select(x => new UserObj
                    {
                        name = x.Fname + " " + x.Lname,
                        thumbnail = x.Thumbnail,
                        type = "child",
                        userId = x.Id.ToString(),
                        email = ""
                    }).ToList();

            foreach (var user in users)
            {
                var imgId = 1; //if user has no profile image, return default image
                if (user.thumbnail != null)
                    imgId = user.thumbnail.ThumbNailId;
                enteties.Add(
                    new
                    {
                        Name = user.name,
                        user.type,
                        user.userId,
                        imgId,
                        user.email
                    });
            }
            foreach (var child in children)
            {
                var imgId = 1; //if there is no profile image, return default profile image
                if (child.thumbnail != null)
                    imgId = child.thumbnail.ThumbNailId;
                enteties.Add(
                    new
                    {
                        Name = child.name,
                        child.type,
                        child.userId,
                        imgId,
                        child.email
                    });
            }
            return Json(enteties);
        }

        /// <summary>
        ///     Publish image
        /// </summary>
        /// <returns>filter result based on parameters</returns>
        [HttpPost]
        public ActionResult PublishImage()
        {
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var date = Request.Form["date"];
            var sponsor = Request.Form["user"];
            var child = Request.Form["child"];
            var filename = Request.Form["filename"];
            var id = int.Parse(Request.Form["id"]);
            var page = Request.Form["page"];
            var pageNumber = 1;
            if (!string.IsNullOrEmpty(page))
                pageNumber = int.Parse(page);

            if (id == 0)
                return HttpNotFound();
            var image = _context.Files.Find(id);
            if (image == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            try
            {
                image.Published = !image.Published;
                _context.SaveChanges();
                ViewBag.Success = "Bildet " + image.FileName + " ble suksessfult oppdatert";
                return FilterImagesResult(search, active, order, filter, date, sponsor, child, filename, pageNumber);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return FilterImagesResult(search, active, order, filter, date, sponsor, child, filename, pageNumber);
            }
        }

        /// <summary>
        ///     Deletes image
        /// </summary>
        /// <returns>image filter result</returns>
        [HttpPost]
        public ActionResult DeleteImage()
        {
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var date = Request.Form["date"];
            var sponsor = Request.Form["user"];
            var child = Request.Form["child"];
            var filename = Request.Form["filename"];
            var id = int.Parse(Request.Form["id"]);
            var page = Request.Form["page"];
            var pageNumber = 1;
            if (!string.IsNullOrEmpty(page))
                pageNumber = int.Parse(page);

            if (id == 0)
                return HttpNotFound();
            var image = _context.Files.Find(id);
            if (image == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var FileName = image.FileName;
            try
            {
                _context.TagBoxs.RemoveRange(image.Tags);
                _context.Entry(image).State = EntityState.Deleted;


                _context.SaveChanges();
                ViewBag.Success = "Bildet " + FileName + " ble suksessfult slettet";
                return FilterImagesResult(search, active, order, filter, date, sponsor, child, filename, pageNumber);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Feil ved sletting av bilde " + FileName + ": " + ex.Message;
                return FilterImagesResult(search, active, order, filter, date, sponsor, child, filename, pageNumber);
            }
        }

        /// <summary>
        ///     edit image
        /// </summary>
        /// <returns>image filter result</returns>
        [HttpPost]
        public ActionResult EditImage()
        {
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var date = Request.Form["date"];
            var sponsor = Request.Form["user"];
            var child = Request.Form["child"];
            var filename = Request.Form["filename"];
            var id = int.Parse(Request.Form["id"]);
            var page = Request.Form["page"];
            var caption = Request.Form["caption"];
            var pageNumber = 1;
            var list = JsonConvert.DeserializeObject<List<DbTables.TagBox>>(Request.Form["tags"]);
            //deserialize and turn json objects (the tags) into actual tag class
            if (!string.IsNullOrEmpty(page))
                pageNumber = int.Parse(page);

            if (id == 0)
                return HttpNotFound();
            var image = _context.Files.Find(id);
            if (image == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            try
            {
                image.Caption = caption;
                _context.TagBoxs.RemoveRange(image.Tags);
                image.Tags = list;
                image.User.Clear(); //clear current users and children, add new one
                image.Children.Clear();
                foreach (var tagBox in list) //get users in tag boxes
                    if (tagBox.type == "user")
                    {
                        var user = _context.Users.Find(tagBox.Id);

                        if (user != null)
                            image.User.Add(user);
                    }
                    else
                    {
                        if (tagBox.Id != null)
                        {
//if nothing is selected id comes in as null, do nothing then

                            var Child = _context.Children.Find(int.Parse(tagBox.Id));
                            if (Child != null) //add child
                                image.Children.Add(Child);
                        }
                    }

                _context.SaveChanges();
                ViewBag.Success = "Bildet " + image.FileName + " ble suksessfult oppdatert";
                return FilterImagesResult(search, active, order, filter, date, sponsor, child, filename, pageNumber);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Feil ved oppdatering av bilde: " + image.FileName + ": " + ex.Message;
                return FilterImagesResult(search, active, order, filter, date, sponsor, child, filename, pageNumber);
            }
        }

        /// <summary>
        ///     Upload images with tags
        /// </summary>
        /// <returns>return success or error message</returns>
        [HttpPost]
        public ActionResult UploadFiles()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
                try
                {
                    var list =
                        JsonConvert.DeserializeObject<List<DbTables.TagBox>>(Request.Form["tags"]);
                    //deserialize and turn json objects (the tags) into actual tag class

                    //  Get all files from Request object  
                    var files = Request.Files;
                    for (var i = 0; i < files.Count; i++)
                    {
                        var file = files[i];

                        var picture = new DbTables.File
                        {
                            FileName = Path.GetFileName(file.FileName),
                            FileType = DbTables.FileType.Picture,
                            ContentType = file.ContentType,
                            Tags = list,
                            User = new List<ApplicationUser>(),
                            Children = new List<DbTables.Child>(),
                            UploadDate = DateTime.Now.Date,
                            Published = true
                        };
                        var caption = Request.Form["caption"];
                        if (!string.IsNullOrEmpty(caption))
                            picture.Caption = caption;
                        foreach (var tagBox in list)
                            //get users and children from tag boxes. add to the relations of the image
                            if (tagBox.type == "user")
                            {
                                var user = _context.Users.Find(tagBox.Id);

                                if (user != null)
                                    picture.User.Add(user);
                            }
                            else
                            {
                                if (tagBox.Id != null)
                                {
//if nothing is selected id comes in as null, do nothing then

                                    var child = _context.Children.Find(int.Parse(tagBox.Id));
                                    if (child != null)
                                        picture.Children.Add(child);
                                }
                            }

                        using (var reader = new BinaryReader(file.InputStream))
                        {
                            picture.Content = reader.ReadBytes(file.ContentLength);
                        }

                        _context.Files.Add(picture);
                        _context.SaveChanges();
                    }
                    // Returns message that successfully uploaded  
                    return Json("Filopplastning var en suksess!");
                }
                catch (Exception ex)
                {
                    return Json("Ooops, det skjedde en feil: " + ex.Message);
                }
            return Json("Ingen filer valgt");
        }
    }

    /// <summary>
    ///     Helper class used to decrease row operations in entity framework
    /// </summary>
    public class UserObj
    {
        public string userId { get; set; }
        public string type { get; set; }
        public DbTables.ThumbNail thumbnail { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public DbTables.Employees employee { get; set; }
    }
}