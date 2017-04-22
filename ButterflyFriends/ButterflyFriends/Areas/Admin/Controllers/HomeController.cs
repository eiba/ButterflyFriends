using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ButterflyFriends.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    [Authorize(Roles = "Owner, Admin")]
    public class HomeController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Images()
        {
            return View(_context.Files.ToList());
        }

        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetUsers()
        {
            var body = Request.InputStream;
            var encoding = Request.ContentEncoding;
            var reader = new StreamReader(body, encoding);
            var json = reader.ReadToEnd();


            var enteties = new List<object>();

            var users = (from s in _context.Users
                          where
                          s.Fname.StartsWith(json) ||
                          s.Lname.StartsWith(json)
                         orderby s.Lname
                          select s).ToList();

            var children = (from s in _context.Children
                            where
                            s.Fname.StartsWith(json) ||
                            s.Lname.StartsWith(json)
                            orderby s.Lname
                            select s).ToList();

            foreach (var user in users)
            {
                var imgId = 0;
                if (user.Thumbnail != null)
                {
                    imgId = user.Thumbnail.ThumbNailId;
                }
                enteties.Add(new {Name = user.Fname +" "+ user.Lname, type = "user", Id =user.Id, imgId = imgId, email=user.Email});
            }
            foreach (var child in children)
            {
                var imgId = 0;
                if (child.Thumbnail != null)
                {
                    imgId = child.Thumbnail.ThumbNailId;
                }
                enteties.Add(new { Name = child.Fname + " " + child.Lname, type = "child", Id = child.Id, imgId = imgId,email=""});
            }
            return Json(enteties);
        }

        [HttpPost]
        public ActionResult UploadFiles()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {

                    List<DbTables.TagBox> list = JsonConvert.DeserializeObject<List<DbTables.TagBox>>(Request.Form["tags"]);

                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {

                        HttpPostedFileBase file = files[i];
                        
                        var picture = new DbTables.File
                        {
                            FileName = Path.GetFileName(file.FileName),
                            FileType = DbTables.FileType.Picture,
                            ContentType = file.ContentType,
                            Tags = list,
                            User = new List<ApplicationUser>(),
                            Children = new List<DbTables.Child>()
                        };
                        
                        foreach (var tagBox in list)
                        {
                            if (tagBox.type =="user") { 
                            ApplicationUser user = _context.Users.Find(tagBox.Id);

                            if (user != null)
                            {
                                picture.User.Add(user);
                            }
                            }
                            else
                            {
                                if (tagBox.Id != null) {//if nothing is selected id comes in as null, do nothing then

                                DbTables.Child child = _context.Children.Find(Int32.Parse(tagBox.Id));
                                if (child != null)
                                {
                                    picture.Children.Add(child);
                                }
                                }
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
            }
            else
            {
                return Json("Ingen filer valgt");
            }
        }

    }
    
}
