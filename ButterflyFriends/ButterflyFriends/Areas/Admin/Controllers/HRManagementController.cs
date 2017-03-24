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
    [Authorize(Roles = "Owner, Admin")]
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
                    Employees = users.Where(s => s.Employee != null).ToPagedList(1, pageSize)
                },
                ChildrenModel = new ChildrenModel {Children = children.ToPagedList(1, pageSize)},
                SponsorModel = new SponsorModel {Sponsors = users.Where(s => s.Employee == null).ToPagedList(1, pageSize)}
            };
            return View(model);
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
                    var type = Request.Form["type"];
                    //ApplicationUser user = _context.Users.Find(userId);
                    var fileId = 0;
                    var employee = new ApplicationUser();
                    var child = new DbTables.Child();
                    IList<DbTables.File> pictures = new List<DbTables.File>();
                    if (type == "employee")
                    {
                        employee = _context.Users.Find(userId);
                        pictures = employee.Pictures;
                    }
                    else if (type == "child")
                    {
                        child = _context.Children.Find(Int32.Parse(userId));
                        pictures = child.Pictures;
                    }

                    //var pictures = user.Pictures;
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
                            var picture = new DbTables.File();
                            //save profile picture
                            if (employee.Email != null)
                            {
                                picture = new DbTables.File
                                {
                                    FileName = Path.GetFileName(file.FileName),
                                    FileType = DbTables.FileType.Profile,
                                    ContentType = file.ContentType,
                                    User = new List<ApplicationUser> {employee}
                                };
                            }
                            else
                            {
                                picture = new DbTables.File
                                {
                                    FileName = Path.GetFileName(file.FileName),
                                    FileType = DbTables.FileType.Profile,
                                    ContentType = file.ContentType,
                                    Children = new List<DbTables.Child> {child}
                                };
                            }
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
                            double ratio = (double) ((double) bmp.Width/(double) bmp.Height);
                            int height = (int) ((double) ProfileImageWidth/ratio);
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

                            int Theight = (int) ((double) ThumbnailImageWidth/ratio);
                            var newThumb = ResizeImage(bmp, ThumbnailImageWidth, Theight);
                            byte[] Tcntnt;
                            using (var stream = new MemoryStream())
                            {
                                newThumb.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                Tcntnt = stream.ToArray();
                            }
                            thumbnail.Content = Tcntnt;
                            _context.ThumbNails.Add(thumbnail);

                            if (employee.Email != null)
                            {
                                employee.Thumbnail = thumbnail;
                                _context.Entry(employee).State = EntityState.Modified;
                            }
                            else
                            {
                                child.Thumbnail = thumbnail;
                                _context.Entry(child).State = EntityState.Modified;
                            }
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
                            double ratio = (double) ((double) bmp.Width/(double) bmp.Height);
                            int height = (int) ((double) ProfileImageWidth/ratio);
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

                            int Theight = (int) ((double) ThumbnailImageWidth/ratio);
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
                    return PartialView("UploadImagePartials/_ProfileImagePartial", new FileModel {FileId = fileId});
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

        public ActionResult EmployeeList(int? employeePage)
        {
            var users = from s in _context.Users
                        where s.Employee != null
                        orderby s.Lname
                        select s;

            int pageNumber = (employeePage ?? 1);
            ViewBag.employeePage = (employeePage ?? 1);

            return PartialView("ListPartials/_EmployeePartial",
                new EmployeeModel {Employees = users.ToPagedList(pageNumber, pageSize)});
        }

        public ActionResult ChildrenList(int? childrenPage)
        {
            var children = from s in _context.Children
                        orderby s.Lname
                        select s;

            int pageNumber = (childrenPage ?? 1);
            ViewBag.childrenPage = (childrenPage ?? 1);

            return PartialView("ListPartials/_ChildrenPartial",
                new ChildrenModel { Children = children.ToPagedList(pageNumber, pageSize) });
        }

        public ActionResult SponsorList(int? sponsorPage)
        {
            var users = from s in _context.Users
                        where s.Employee == null
                        orderby s.Lname
                        select s;

            int pageNumber = (sponsorPage ?? 1);
            ViewBag.sponsorPage = (sponsorPage ?? 1);

            return PartialView("ListPartials/_SponsorPartial",
                new SponsorModel { Sponsors = users.ToPagedList(pageNumber, pageSize) });
        }
        /****************Detail functions**************/
        [HttpGet]
        public ActionResult showEmployeeDetails(string id, string type)
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
            var pictures = user.Pictures;

            if (pictures.Any())
            {
                foreach (var pic in pictures)
                {
                    if (pic.FileType == DbTables.FileType.Profile)
                    {
                        return PartialView("DetailPartials/_EmployeeDetailsPartial",new EmployeeDetailsModel { File = new FileModel { FileId = pic.FileId, Type = "employee" },User = user});
                    }
                }
            }
            return PartialView("DetailPartials/_EmployeeDetailsPartial", new EmployeeDetailsModel { File = new FileModel { Type = "employee" }, User = user });


        }
        [HttpGet]
        public ActionResult showSponsorDetails(string id, string type)
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
            var pictures = user.Pictures;

            if (pictures.Any())
            {
                foreach (var pic in pictures)
                {
                    if (pic.FileType == DbTables.FileType.Profile)
                    {
                        return PartialView("DetailPartials/_SponsorDetailsPartial", new EmployeeDetailsModel { File = new FileModel { FileId = pic.FileId, Type = "employee" }, User = user });
                    }
                }
            }
            return PartialView("DetailPartials/_SponsorDetailsPartial", new EmployeeDetailsModel { File = new FileModel { Type = "employee" }, User = user });


        }
        [HttpGet]
        public ActionResult showChildDetails(int id, string type)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbTables.Child child = _context.Children.Find(id);
            if (child == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var pictures = child.Pictures;

            if (pictures.Any())
            {
                foreach (var pic in pictures)
                {
                    if (pic.FileType == DbTables.FileType.Profile)
                    {
                        return PartialView("DetailPartials/_ChildDetailsPartial", new ChildDetailsModel { File = new FileModel { FileId = pic.FileId, Type = "child" }, Child = child });
                    }
                }
            }
            return PartialView("DetailPartials/_ChildDetailsPartial", new ChildDetailsModel { File = new FileModel { Type = "child" },Child = child});
        }

        /************************************************/
        [HttpGet]
        public ActionResult showEmployeeEdit(string id, int? employeePage)
        {
            ViewBag.employeePage = (employeePage ?? 1);
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
                Email = user.Email,
                Position = user.Employee.Position,
                AccountNumber = user.Employee.AccountNumber

            };

            return PartialView("EditPartials/_EmployeeEditPartial", model);
        }

        [HttpGet]
        public ActionResult showSponsorEdit(string id, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);
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

            return PartialView("EditPartials/_SponsorEditPartial", model);
        }

        [HttpGet]
        public ActionResult showChildEdit(int? id, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbTables.Child child = _context.Children.Find(id);
            if (child == null)
            {
                return HttpNotFound();
            }
            ChildCreateModel model;
            if (child.SponsorId == null)
            {
                model = new ChildCreateModel
                {
                    Child = child
                };
            }
            else
            {
                model = new ChildCreateModel
                {
                    Child = child,
                    SponsorName = child.User.Fname + " " + child.User.Lname
                };
            }
            return PartialView("EditPartials/_ChildEditPartial", model);

        }

        [HttpPost]
        public ActionResult ChildEdit(ChildCreateModel model, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            if (ModelState.IsValid)
            {
                var sponsormsg = "";
                if (model.Child.Id == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var child = _context.Children.Find(model.Child.Id);
                if (child == null)
                {
                    return HttpNotFound();
                }
                if (model.Child.DoB > DateTime.Now)
                {
                    var dateErrorChildren = from s in _context.Children
                                        orderby s.Lname
                                        select s;
                    ViewBag.Error = "Barnet kan ikke være født i fremtiden!";

                    return PartialView("ListPartials/_ChildrenPartial", new ChildrenModel { Children = dateErrorChildren.ToPagedList((childrenPage ?? 1), pageSize) });
                }

                child.DoB = model.Child.DoB;
                child.Fname = model.Child.Fname;
                child.Lname = model.Child.Lname;
                if (model.Child.SponsorId != null && model.SponsorName != null)
                {
                    var user = _context.Users.Find(model.Child.SponsorId);
                    if (user != null)
                    {
                        if (user.Fname + " " + user.Lname != model.SponsorName)
                        {
                            //child.SponsorId = null;
                            sponsormsg = ", men fant ikke bruker " + model.SponsorName;
                        }
                        else
                        {
                            child.SponsorId = model.Child.SponsorId;
                        }
                    }
                }
                else if (model.Child.SponsorId == null && model.SponsorName != null)
                {
                    sponsormsg = ", men fant ikke bruker " + model.SponsorName;
                }
                else if (model.SponsorName == null)
                {
                    child.SponsorId = null;
                }
                try
                {
                    _context.Entry(child).State = EntityState.Modified;
                    _context.SaveChanges();

                    var successChildren = from s in _context.Children
                                          orderby s.Lname
                                          select s;

                    ViewBag.Success = "Fadderbarnet " + model.Child.Fname + " " + model.Child.Lname + " ble oppdatert" +sponsormsg;
                    ViewBag.Id = model.Child.Id;
                    return PartialView("ListPartials/_ChildrenPartial",
                        new ChildrenModel { Children = successChildren.ToPagedList((childrenPage ?? 1), pageSize) });
                }
                catch (EntityException ex)
                {
                    var entityErrorChildren = from s in _context.Children
                                        orderby s.Lname
                                        select s;

                    ViewBag.Error = "Error: "+ex.Message;
                    ViewBag.Id = model.Child.Id;

                    return PartialView("ListPartials/_ChildrenPartial", new ChildrenModel { Children = entityErrorChildren.ToPagedList((childrenPage ?? 1), pageSize) });
                }

            }
            var errorChildren = from s in _context.Children
                                orderby s.Lname
                                select s;

            ViewBag.Error = "Oops! Ugyldige verdier";
            ViewBag.Id = model.Child.Id;

            return PartialView("ListPartials/_ChildrenPartial",new ChildrenModel { Children = errorChildren.ToPagedList((childrenPage ?? 1), pageSize) });
        }
        [HttpGet]
        public ActionResult showEmployeeCreate(int? employeePage)
        {
            ViewBag.employeePage = (employeePage ?? 1);

            return PartialView("CreatePartials/_EmployeeCreatePartial", new RegisterViewModel());
        }
        [HttpGet]
        public ActionResult showSponsorCreate(int? sponsorPage)
        {
            ViewBag.employeePage = (sponsorPage ?? 1);

            return PartialView("CreatePartials/_SponsorCreatePartial", new RegisterViewModel());
        }
        public ActionResult showChildCreate(int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);

            return PartialView("CreatePartials/_ChildCreatePartial", new ChildCreateModel());
        }

        /************************deactivate functions**********************/

        [HttpGet]
        public async Task<ActionResult> employeeDeactivate(string id, int? employeePage)
        {
            ViewBag.employeePage = (employeePage ?? 1);
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

                ViewBag.Error = "Noe gikk galt - " + result.Errors;
                ViewBag.Id = user.Id;
                ViewBag.Worked = false;
            }
            var users = from s in _context.Users
                        where s.Employee != null
                        orderby s.Lname
                        select s;
            return PartialView("ListPartials/_EmployeePartial",
                new EmployeeModel {Employees = users.ToPagedList((employeePage ?? 1), pageSize)});
        }

        [HttpGet]
        public async Task<ActionResult> sponsorDeactivate(string id, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);
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

                ViewBag.Error = "Noe gikk galt - " + result.Errors;
                ViewBag.Id = user.Id;
                ViewBag.Worked = false;
            }
            var users = from s in _context.Users
                        where s.Employee == null
                        orderby s.Lname
                        select s;
            return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = users.ToPagedList((sponsorPage ?? 1), pageSize) });

        }

        [HttpGet]
        public ActionResult childDeactivate(int id, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbTables.Child child = _context.Children.Find(id);
            if (child == null)
            {
                return HttpNotFound();
            }
            child.isActive = false;
            try
            {
            _context.Entry(child).State = EntityState.Modified;
            _context.SaveChanges();

                var children = from s in _context.Children
                            orderby s.Lname
                            select s;
                ViewBag.Success = "Barnet " + child.Fname +" "+child.Lname+ " har blitt deaktivert.";
                ViewBag.Id = child.Id;
                return PartialView("ListPartials/_ChildrenPartial",
                    new ChildrenModel {Children = children.ToPagedList((childrenPage ?? 1), pageSize)});
            }
            catch (EntityException ex)
            {
                var children = from s in _context.Children
                               orderby s.Lname
                               select s;

                ViewBag.Error = "Noe gikk galt "+ex.Message;
                ViewBag.Id = child.Id;
                return PartialView("ListPartials/_ChildrenPartial",
                    new ChildrenModel { Children = children.ToPagedList((childrenPage ?? 1), pageSize) });
            }
            
        }

        /***************************************/

        /************************activate functions**********************/
        [HttpGet]
        public async Task<ActionResult> employeeActivate(string id, int? employeePage)
        {
            ViewBag.employeePage = (employeePage ?? 1);
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
                        where s.Employee != null
                        orderby s.Lname
                        select s;

            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((employeePage ?? 1),pageSize) });
        }
        [HttpGet]
        public async Task<ActionResult> sponsorActivate(string id, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);
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
                        where s.Employee == null
                        orderby s.Lname
                        select s;

            return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = users.ToPagedList((sponsorPage ?? 1), pageSize) });
        }
        [HttpGet]
        public async Task<ActionResult> childActivate(int id, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbTables.Child child = _context.Children.Find(id);
            if (child == null)
            {
                return HttpNotFound();
            }
            child.isActive = true;
            try
            {
                _context.Entry(child).State = EntityState.Modified;
                _context.SaveChanges();

                var children = from s in _context.Children
                               orderby s.Lname
                               select s;
                ViewBag.Success = "Barnet " + child.Fname + " " + child.Lname + " har blitt aktivert.";
                ViewBag.Id = child.Id;
                return PartialView("ListPartials/_ChildrenPartial",
                    new ChildrenModel { Children = children.ToPagedList((childrenPage ?? 1), pageSize) });
            }
            catch (EntityException ex)
            {
                var children = from s in _context.Children
                               orderby s.Lname
                               select s;

                ViewBag.Error = "Noe gikk galt " + ex.Message;
                ViewBag.Id = child.Id;
                return PartialView("ListPartials/_ChildrenPartial",
                    new ChildrenModel { Children = children.ToPagedList((childrenPage ?? 1), pageSize) });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditEmployee(changeUserInfo model, int? employeePage)
        {
            ViewBag.employeePage = (employeePage ?? 1);
            var users = from s in _context.Users
                        where s.Employee != null
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
                            
                            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((employeePage ?? 1), pageSize) });
                    }
                }
                }
                //so far so good, change the details of the user
                user.Fname = model.Fname;
                user.Lname = model.Lname;
                user.Phone = model.Phone;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.Employee.AccountNumber = model.AccountNumber;
                user.Employee.Position = model.Position;

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
                                       where s.Employee != null
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
                           
                            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = successUsers.ToPagedList((employeePage ?? 1), pageSize) });
                        }
                    }

                    ViewBag.Success = "Brukeren "+ user.Email+" ble oppdatert.";
                    ViewBag.Id = user.Id;

                    return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = successUsers.ToPagedList((employeePage ?? 1), pageSize) });
                    
                }
               

                ViewBag.Id = user.Id;
                    ViewBag.Error = "Noe gikk galt "+result.Errors;
                    return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((employeePage ?? 1), pageSize) });
                
            }


            ViewBag.Id = model.Id;
            ViewBag.Error = "Noe gikk galt";
            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((employeePage ?? 1), pageSize) });
            


        }

        [HttpPost]
        public async Task<ActionResult> EditSponsor(changeUserInfo model, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);
            var users = from s in _context.Users
                        where s.Employee == null
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

                if (results.Any())
                {
                    foreach (var appUser in results)
                    {
                        if (appUser.Id != model.Id)
                        {
                            ViewBag.Id = user.Id;
                            ViewBag.Error = "Emailen er allerede i bruk";
                            return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = users.ToPagedList((sponsorPage ?? 1), pageSize) });
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
                                       where s.Employee == null
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
                            return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = successUsers.ToPagedList((sponsorPage ?? 1), pageSize) });
                        }
                    }

                    ViewBag.Success = "Brukeren " + user.Email + " ble oppdatert.";
                    ViewBag.Id = user.Id;
                    return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = successUsers.ToPagedList((sponsorPage ?? 1), pageSize) });

                }


                ViewBag.Id = user.Id;
                ViewBag.Error = "Noe gikk galt " + result.Errors;
                return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = users.ToPagedList((sponsorPage ?? 1), pageSize) });
            }


            ViewBag.Id = model.Id;
            ViewBag.Error = "Noe gikk galt";
            return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = users.ToPagedList((sponsorPage ?? 1), pageSize) });



        }

        [HttpPost]
        public async Task<ActionResult> CreateEmployee(RegisterViewModel model, int? employeePage)
        {
            ViewBag.employeePage = (employeePage ?? 1);
            var users = from s in _context.Users
                        where s.Employee != null
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
                                    new EmployeeModel { Employees = users.ToPagedList((employeePage ?? 1), pageSize) });
                           
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
                    RoleNr = 0,
                    Employee = new DbTables.Employees {AccountNumber = model.AccountNumber,Position = model.Position}
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
                        return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((employeePage ?? 1), pageSize) });

                    }

                }
                user.Adress = UserAdress;

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var successUsers = from s in _context.Users
                                       where s.Employee != null
                                orderby s.Lname
                                select s;
                    userManager.AddToRole(user.Id, user.AccessLvL);
                    ViewBag.Success = "Brukeren "+user.Email+" ble lagt til i databasen";
                    ViewBag.Id = user.Id;
                    //var users = _context.Users.ToList();
                    return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = successUsers.ToPagedList((employeePage ?? 1), pageSize) });
                }
                ViewBag.Error = "Noe gikk galt "+ result.Errors;
                return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((employeePage ?? 1), pageSize) });
            }

            ViewBag.Error = "Noe gikk galt";
            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = users.ToPagedList((employeePage ?? 1), pageSize) });
        }

        [HttpPost]
        public async Task<ActionResult> CreateSponsor(RegisterViewModel model, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);
            var users = from s in _context.Users
                        where s.Employee==null
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

                            return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = users.ToPagedList((sponsorPage ?? 1), pageSize) });


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
                    AccessLvL = "Sponsor",
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
                        return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = users.ToPagedList((sponsorPage ?? 1), pageSize) });

                    }

                }
                user.Adress = UserAdress;

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var successUsers = from s in _context.Users
                                       where s.Employee == null
                                       orderby s.Lname
                                       select s;
                    userManager.AddToRole(user.Id, user.AccessLvL);
                    ViewBag.Success = "Brukeren " + user.Email + " ble lagt til i databasen";
                    ViewBag.Id = user.Id;
                    //var users = _context.Users.ToList();
                    return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = successUsers.ToPagedList((sponsorPage ?? 1), pageSize) });

                }
                ViewBag.Error = "Noe gikk galt " + result.Errors;
                return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = users.ToPagedList((sponsorPage ?? 1), pageSize) });

            }

            ViewBag.Error = "Noe gikk galt";
            return PartialView("ListPartials/_SponsorPartial", new SponsorModel { Sponsors = users.ToPagedList((sponsorPage ?? 1), pageSize) });
        }

        [HttpPost]
        public ActionResult CreateChild(ChildCreateModel model, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            if (ModelState.IsValid)
            {
                var sponsormsg = "";
                if (model.Child.SponsorId != null) { 
                var user = _context.Users.Find(model.Child.SponsorId);
                if (user != null)
                {
                    if (user.Fname +" "+user.Lname != model.SponsorName)
                    {
                        model.Child.SponsorId = null;
                            sponsormsg = ", men fant ikke bruker " + model.SponsorName;
                        }
                }
                }
                else if (model.Child.SponsorId == null && model.SponsorName != null)
                {
                    sponsormsg = ", men fant ikke bruker " + model.SponsorName;
                }
                model.Child.isActive = true;
                if (model.Child.DoB > DateTime.Now)
                {
                    var errorChildren = from s in _context.Children
                                   orderby s.Lname
                                   select s;
                    ViewBag.Error = "Barnet kan ikke være født i fremtiden!";

                    return PartialView("ListPartials/_ChildrenPartial", new ChildrenModel { Children = errorChildren.ToPagedList((childrenPage ?? 1), pageSize) });
                }
                try
                {
                    _context.Children.Add(model.Child);
                    _context.SaveChanges();

                    var successChildren = from s in _context.Children
                        orderby s.Lname
                        select s;

                    ViewBag.Success = "Fadderbarnet " + model.Child.Fname + " " + model.Child.Lname + " ble lagt til i databasen"+sponsormsg;
                    ViewBag.Id = model.Child.Id;
                    return PartialView("ListPartials/_ChildrenPartial",
                        new ChildrenModel {Children = successChildren.ToPagedList((childrenPage ?? 1), pageSize)});
                }
                catch (EntityException ex)
                {
                    ViewBag.Error = "Error: "+ex.Message;
                    var errorChildren = from s in _context.Children
                                          orderby s.Lname
                                          select s;

                    return PartialView("ListPartials/_ChildrenPartial",
                        new ChildrenModel { Children = errorChildren.ToPagedList((childrenPage ?? 1), pageSize) });
                }
            }

            var children = from s in _context.Children
                           orderby s.Lname
                           select s;
            ViewBag.Error = "Oops, ikke gyldige verdier!";

            return PartialView("ListPartials/_ChildrenPartial", new ChildrenModel { Children = children.ToPagedList((childrenPage ?? 1), pageSize) });
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

            foreach (var user in users)
            {
                var imgId = 0;
                if (user.Thumbnail != null)
                {
                    imgId = user.Thumbnail.ThumbNailId;
                }
                enteties.Add(new { Name = user.Fname + " " + user.Lname, Id = user.Id, imgId = imgId });
            }
            return Json(enteties);
        }
    }
}