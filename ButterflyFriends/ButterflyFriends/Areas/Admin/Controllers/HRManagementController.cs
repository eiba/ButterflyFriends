using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ButterflyFriends.Areas.Admin.Models;
using ButterflyFriends.Areas.Admin.Models.HRmanagementModels;
using ButterflyFriends.Controllers;
using ButterflyFriends.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using Newtonsoft.Json;
using PagedList;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    public class HRManagementController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();
        public int pageSize = 10;

        // GET: Admin/HRManagement
        public ActionResult Index()
        {
            var users = from s in _context.Users
                           orderby s.Lname
                           select s;

            var children = from s in _context.Children
                           orderby s.Lname
                           select s;

            var model = new HRManagamentModel
            {
                EmployeeModel = new EmployeeModel
                {
                    Employees = users.ToPagedList(1, pageSize)
                },
                ChildrenModel = new ChildrenModel {Children = children.ToPagedList(1, pageSize) }
    };
            ViewBag.CurrentPage = 1;
            return View(model);
        }

        public ActionResult ShowProfilePictureUpload(string id, int? page)
        {
            ViewBag.page = page;
            ApplicationUser user = _context.Users.Find(id);

            var pictures = user.Pictures;
            if (pictures.Any()) { 
            foreach (var pic in pictures)
            {
                if (pic.FileType == DbTables.FileType.Profile)
                {
                        return PartialView("UploadImagePartials/_EmployeeUploadImage", new FileModel {FileId = pic.FileId, userId = id});
                    }
            }
            }
            return PartialView("UploadImagePartials/_EmployeeUploadImage",new FileModel {userId = id});
        }

        [HttpPost]
        public ActionResult ProfilePictureUpload()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    var ProfileImageWidth = 300;
                    var ThumbnailImageWidth = 40;
                    var userId = Request.Form["userid"];
                    ApplicationUser user = _context.Users.Find(userId);
                    var fileId = 0;

                    var pictures = user.Pictures;
                    var profPic = new DbTables.File();
                    var thumbNail = new DbTables.ThumbNail();
                    if (pictures.Any())
                    {
                        foreach (var pic in pictures)
                        {
                            if (pic.FileType == DbTables.FileType.Profile)
                            {
                                profPic = pic;
                                thumbNail = pic.ThumbNail;
                            }
                        }
                    }
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {

                        HttpPostedFileBase file = files[i];

                        if (profPic.Content == null)
                        {
                            //save profile picture
                            var picture = new DbTables.File
                            {
                                FileName = Path.GetFileName(file.FileName),
                                FileType = DbTables.FileType.Profile,
                                ContentType = file.ContentType,
                                User = new List<ApplicationUser> {user}
                            };

                            byte[] content; 
                            using (var reader = new BinaryReader(file.InputStream))
                            {

                                content = reader.ReadBytes(file.ContentLength);
                            }

                            Bitmap bmp;
                            using (var ms = new MemoryStream(content))
                            {
                                bmp = new Bitmap(ms);
                            }
                            double ratio = (double)((double)bmp.Width / (double)bmp.Height);
                            int height = (int)((double)ProfileImageWidth / ratio);
                            var newImg = ResizeImage(bmp, ProfileImageWidth, height);

                            byte[] cntnt;
                            using (var stream = new MemoryStream())
                            {
                                newImg.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                cntnt = stream.ToArray();
                            }
                            picture.Content = cntnt;

                            _context.Files.Add(picture);

                            //save thumbnail
                            var thumbnail = new DbTables.ThumbNail
                            {
                                ThumbNailName = Path.GetFileName(file.FileName),
                                ContentType = file.ContentType,
                                FileType = DbTables.FileType.Thumbnail,
                                File = picture
                                //User = new List<ApplicationUser> {user}
                            };

                            int Theight = (int)((double)ThumbnailImageWidth / ratio);
                            var newThumb = ResizeImage(bmp, ThumbnailImageWidth, Theight);
                            byte[] Tcntnt;
                            using (var stream = new MemoryStream())
                            {
                                newThumb.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                Tcntnt = stream.ToArray();
                            }
                            thumbnail.Content = Tcntnt;
                            _context.ThumbNails.Add(thumbnail);

                            user.Thumbnail = thumbnail;
                            _context.Entry(user).State = EntityState.Modified;

                            _context.SaveChanges();
                            fileId = picture.FileId;
                        }
                        else
                        {
                            //Profile picture
                            profPic.FileName = Path.GetFileName(file.FileName);
                            profPic.ContentType = file.ContentType;

                            byte[] content;
                            using (var reader = new BinaryReader(file.InputStream))
                            {

                                content = reader.ReadBytes(file.ContentLength);
                            }

                            Bitmap bmp;
                            using (var ms = new MemoryStream(content))
                            {
                                bmp = new Bitmap(ms);
                            }
                            double ratio = (double)((double)bmp.Width / (double)bmp.Height);
                            int height = (int)((double)ProfileImageWidth / ratio);
                            var newImg = ResizeImage(bmp, ProfileImageWidth, height);

                            byte[] cntnt;
                            using (var stream = new MemoryStream())
                            {
                                newImg.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                cntnt = stream.ToArray();
                            }
                            profPic.Content = cntnt;

                            _context.Entry(profPic).State = EntityState.Modified;

                            //thumbnail
                            thumbNail.ThumbNailName = Path.GetFileName(file.FileName);
                            thumbNail.ContentType = file.ContentType;

                            int Theight = (int)((double)ThumbnailImageWidth / ratio);
                            var newThumb = ResizeImage(bmp, ThumbnailImageWidth, Theight);
                            byte[] Tcntnt;
                            using (var stream = new MemoryStream())
                            {
                                newThumb.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                Tcntnt = stream.ToArray();
                            }
                            thumbNail.Content = Tcntnt;
                            _context.Entry(thumbNail).State = EntityState.Modified;

                            _context.SaveChanges();
                            fileId = profPic.FileId;
                        }

                    }
                    // Returns message that successfully uploaded  
                    return PartialView("UploadImagePartials/_ProfileImagePartial",new FileModel {FileId = fileId});
                    //return Json("Filopplastning var en suksess!");
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
        /// <summary>
        /// Rezises image so that it will be be compressed with smaller dimensions
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics gfx = Graphics.FromImage(resizedImage))
            {
                gfx.DrawImage(image, new Rectangle(0, 0, width, height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            return resizedImage;
        }

        public ActionResult EmployeeList(int? page)
        {
            var users = from s in _context.Users
                           orderby s.Lname
                           select s;

            int pageNumber = (page ?? 1);
            ViewBag.page = page;

            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList(pageNumber, pageSize) });
        }
        
        [HttpGet]
        public ActionResult showEmployeeDetails(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return PartialView("DetailPartials/_EmployeeDetailsPartial",user);
        }

        [HttpGet]
        public ActionResult showEmployeeEdit(string id, int? page)
        {
            ViewBag.page = page;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new changeUserInfo
            {
                Fname = user.Fname,
                Lname = user.Lname,
                Id = user.Id,
                Phone = user.Phone,
                PostCode = user.Adress.PostCode,
                City = user.Adress.City,
                StreetAdress = user.Adress.StreetAdress,
                State = user.Adress.County,
                Email = user.Email

            };

            return PartialView("EditPartials/_EmployeeEditPartial",model);
        }
        [HttpGet]
        public ActionResult showEmployeeCreate(int? page)
        {
            ViewBag.page = page;

            return PartialView("CreatePartials/_EmployeeCreatePartial", new RegisterViewModel());
        }

        [HttpGet]
        public async Task<ActionResult> employeeDeactivate(string id, int? page)
        {
            ViewBag.page = page;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var store = new UserStore<ApplicationUser>(_context);
            var manager = new UserManager<ApplicationUser>(store);

            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = new DateTime(9999, 12, 30);
            user.IsEnabeled = false;

            var result = await manager.UpdateAsync(user);
            _context.SaveChanges();

            if (result.Succeeded)
            {
                ViewBag.Success = "Brukeren " + user.Email + " har blitt deaktivert.";
                ViewBag.Id = user.Id;
            }
            else
            {
                
                ViewBag.Error = "Noe gikk galt - "+ result.Errors;
                ViewBag.Id = user.Id;
                ViewBag.Worked = false;
            }
            var users = from s in _context.Users
                        orderby s.Lname
                        select s;
            return PartialView("ListPartials/_EmployeePartial",new EmployeeModel { Employees = users.ToPagedList((page ?? 1), pageSize) });
        }

        [HttpGet]
        public async Task<ActionResult> employeeActivate(string id, int? page)
        {
            ViewBag.page = page;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var store = new UserStore<ApplicationUser>(_context);
            var manager = new UserManager<ApplicationUser>(store);

            user.LockoutEnabled = false;
            user.IsEnabeled = true;

            var result = await manager.UpdateAsync(user);
            _context.SaveChanges();

            if (result.Succeeded)
            {
                ViewBag.Success = "Brukeren " + user.Email + " har blitt aktivert.";
                ViewBag.Id = user.Id;
            }
            else
            {

                ViewBag.Error = "Noe gikk galt - " + result.Errors;
                ViewBag.Id = user.Id;
            }
            var users = from s in _context.Users
                        orderby s.Lname
                        select s;

            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((page ?? 1),pageSize) });
        }

        [HttpPost]
        public async Task<ActionResult> EditEmployee(changeUserInfo model, int? page)
        {
            ViewBag.page = page;
            var users = from s in _context.Users
                        orderby s.Lname
                        select s;
            if (ModelState.IsValid)
            {
                

                var store = new UserStore<ApplicationUser>(_context);
                var manager = new UserManager<ApplicationUser>(store);
                ApplicationUser user = manager.FindById(model.Id); //the user to be edited
                if (user == null)
                {
                    return HttpNotFound();
                }

                var results = (from s in _context.Users
                              where
                                  s.Email.Contains(model.Email)
                              select s).ToList();

                if (results.Any()) { 
                foreach (var appUser in results)
                {
                    if (appUser.Id != model.Id)
                    {
                        ViewBag.Id = user.Id;
                        ViewBag.Error = "Emailen er allerede i bruk";
                            
                            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((page ?? 1), pageSize) });
                    }
                }
                }
                //so far so good, change the details of the user
                user.Fname = model.Fname;
                user.Lname = model.Lname;
                user.Phone = model.Phone;
                user.Email = model.Email;
                user.UserName = model.Email;

                var newAdress = new DbTables.Adresses
                {
                    City = model.City,
                    StreetAdress = model.StreetAdress,
                    County = model.State,
                    PostCode = model.PostCode
                };
                var adress = AdressExist(newAdress);

                if (adress == null)
                {
                    user.Adress = newAdress;
                    _context.Adresses.Add(newAdress);
                    _context.SaveChanges();

                }
                else if (user.Adress == adress)
                {
                    //do nothing
                }
                else
                {
                    user.Adress = adress;
                }

                var result = await manager.UpdateAsync(user); //update the user in the databse
                _context.SaveChanges();

                if (result.Succeeded) //if update succeeds
                {
                    var successUsers = from s in _context.Users
                                orderby s.Lname
                                select s;
                    if (model.Password != null)
                    {
                        var provider = new DpapiDataProtectionProvider("ButterflyFriends");
                        manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("Passwordresetting"));
                        string resetToken = await manager.GeneratePasswordResetTokenAsync(model.Id);
                        var Presult = await manager.ResetPasswordAsync(model.Id, resetToken, model.Password);
                        if (!Presult.Succeeded)
                        {
                            ViewBag.Error = "Passordet må matche kriteriene";
                            ViewBag.Id = user.Id;
                           
                            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = successUsers.ToPagedList((page ?? 1), pageSize) });
                        }
                    }

                    ViewBag.Success = "Brukeren "+ user.Email+" ble oppdatert.";
                    ViewBag.Id = user.Id;

                    return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = successUsers.ToPagedList((page ?? 1), pageSize) });
                    
                }
               

                ViewBag.Id = user.Id;
                    ViewBag.Error = "Noe gikk galt "+result.Errors;
                    return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((page ?? 1), pageSize) });
                
            }


            ViewBag.Id = model.Id;
            ViewBag.Error = "Noe gikk galt";
            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((page ?? 1), pageSize) });
            


        }

        [HttpPost]
        public async Task<ActionResult> CreateEmployee(RegisterViewModel model, int? page)
        {
            ViewBag.page = page;
            var users = from s in _context.Users
                        orderby s.Lname
                        select s;
            if (ModelState.IsValid)
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
                var results = (from s in _context.Users
                              where
                                  s.Email.Contains(model.Email)
                              select s).ToList();
                
                if (results.Any())
                {
                    foreach (var r in results)
                    {
                        if (r.Email == model.Email)
                        {
                            
                            ViewBag.Error = "Emailen er allerede i bruk.";
                           
                                return PartialView("ListPartials/_EmployeePartial",
                                    new EmployeeModel { Employees = users.ToPagedList((page ?? 1), pageSize) });
                           
                        }
                    }

                }
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Fname = model.Fname,
                    Lname = model.Lname,
                    Phone = model.Phone,
                    AccessLvL = "Employee",
                    IsEnabeled = true,
                    RoleNr = 0
                };
                var Adress = new DbTables.Adresses
                {
                    StreetAdress = model.StreetAdress,
                    City = model.City,
                    County = model.State,
                    PostCode = model.PostCode
                };
                
                var UserAdress = AdressExist(Adress);
                if (UserAdress == null)
                {
                    UserAdress = Adress;
                    try
                    {
                        _context.Adresses.Add(UserAdress);
                        _context.SaveChanges();
                    }
                    catch (EntityException ex)
                    {
                        ViewBag.Error = "Noe gikk galt " + ex.Message;
                        return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((page ?? 1), pageSize) });

                    }

                }
                user.Adress = UserAdress;

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var successUsers = from s in _context.Users
                                orderby s.Lname
                                select s;
                    userManager.AddToRole(user.Id, user.AccessLvL);
                    ViewBag.Success = "Brukeren "+user.Email+" ble lagt til i databasen";
                    ViewBag.Id = user.Id;
                    //var users = _context.Users.ToList();
                    return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = successUsers.ToPagedList((page ?? 1), pageSize) });
                }
                ViewBag.Error = "Noe gikk galt "+ result.Errors;
                return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((page ?? 1), pageSize) });
            }

            ViewBag.Error = "Noe gikk galt";
            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((page ?? 1), pageSize) });
        }

        public DbTables.Adresses AdressExist(DbTables.Adresses adress)
        {
            var adresses = _context.Set<DbTables.Adresses>();
            foreach (var Adress in adresses)
            {
                if (adress.StreetAdress == Adress.StreetAdress && adress.PostCode == Adress.PostCode & adress.City == Adress.City && adress.County == Adress.County)
                {
                    return Adress;
                }
            }
            return null;
        }
    }
}