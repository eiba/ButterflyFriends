using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
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


            var users = new List<object>();

            var results = from s in _context.Users
                          where
                          s.Fname.StartsWith(json) ||
                          s.Lname.StartsWith(json)
                          orderby s.Lname
                          select s;

            foreach (var user in results)
            {
                users.Add(new {Name = user.Fname +" "+ user.Lname, Id=user.Id});
            }
            return Json(users);
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
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
    
                        var picture = new DbTables.File
                        {
                            FileName = Path.GetFileName(file.FileName),
                            FileType = DbTables.FileType.Picture,
                            ContentType = file.ContentType,
                            Tags = list,
                            User = new List<ApplicationUser>()
                        };
                        
                        foreach (var tagBox in list)
                        {
                            ApplicationUser user = _context.Users.Find(tagBox.Id);
                            
                            if (user != null) { 
                                picture.User.Add(user);
                            }
                        }
                        
                        using (var reader = new BinaryReader(file.InputStream))
                        {
                            picture.Content = reader.ReadBytes(file.ContentLength);
                        }

                        _context.Files.Add(picture);
                        _context.SaveChanges();
                        /*
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Uploads/"), fname);
                        file.SaveAs(fname);*/
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
